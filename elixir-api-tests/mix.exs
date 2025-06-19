defmodule ElixirApiTests.MixProject do
  use Mix.Project

  def project do
    [
      app: :elixir_api_tests,
      version: "0.1.0",
      elixir: "~> 1.15",
      elixirc_paths: elixirc_paths(Mix.env()),
      start_permanent: Mix.env() == :prod,
      deps: deps()
    ]
  end

  # Specifies which paths to compile per environment.
  defp elixirc_paths(:test), do: ["lib", "test"]
  defp elixirc_paths(_), do: ["lib"]

  # Run "mix help compile.app" to learn about applications.
  def application do
    [
      extra_applications: [:logger, :httpoison]
    ]
  end

  # Run "mix help deps" to learn about dependencies.
  defp deps do
    [
      {:httpoison, "~> 2.0"},
      {:jason, "~> 1.4"},
      {:bypass, "~> 2.1", only: :test},
      # Property-based testing utilities
      {:stream_data, "~> 0.5", only: :test},
      {:ex_doc, "~> 0.28", only: :dev, runtime: false}
    ]
  end
end