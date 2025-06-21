defmodule TestReportGenerator do
  @moduledoc """
  Simple helper for writing test results during execution.
  Each call appends a map containing test info to a JSON report
  and also appends a readable line to a text file.
  """

  require Logger

  @app :elixir_api_tests
  @json_file "test_report.json"
  @text_file "test_report.txt"

  @doc """
  Appends a test result map to the JSON and text reports.
  Returns :ok or {:error, reason}.
  """
  @spec append_result(map()) :: :ok | {:error, term()}
  def append_result(%{} = result) do
    output_dir = Application.get_env(@app, :report_output, "reports")
    File.mkdir_p!(output_dir)
    json_path = Path.join(output_dir, @json_file)
    text_path = Path.join(output_dir, @text_file)

    results =
      with {:ok, body} <- File.read(json_path),
           {:ok, data} <- Jason.decode(body) do
        data
      else
        _ -> []
      end

    updated = results ++ [result]
    try do
      File.write!(json_path, Jason.encode!(updated, pretty: true))
      File.write!(text_path, format_text(result), [:append])
      :ok
    rescue
      e ->
        Logger.error("Failed to write test report: #{inspect(e)}")
        {:error, e}
    end
  end

  @doc """
  Formats a test result map as a readable string for the text report.
  """
  @spec format_text(map()) :: String.t()
  defp format_text(%{name: name, status: status, message: message}) do
    "#{name} - #{status}: #{message}\n"
  end

  defp format_text(map) do
    Jason.encode!(map) <> "\n"
  end
end
