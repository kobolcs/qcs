defmodule RestCountriesAsiaValidatorTest do
  use ExUnit.Case, async: true
  use ExUnitProperties
  alias TestReportGenerator
  alias Plug.Conn

  @moduletag timeout: 30000
  @default_endpoint "https://restcountries.com/v3.1/region/asia"

  setup do
    if System.get_env("USE_LIVE_REST_COUNTRIES") == "1" do
      {:ok, endpoint: @default_endpoint}
    else
      bypass = Bypass.open()

      mock_countries =
        for i <- 1..35 do
          %{"name" => %{"common" => "Mock#{i}"}, "region" => "Asia", "area" => 1000 + i}
        end

      json = Jason.encode!(mock_countries)

      Bypass.stub(bypass, "GET", "/v3.1/region/asia", fn conn ->
        Conn.put_resp_header(conn, "content-type", "application/json")
        |> Conn.resp(200, json)
      end)

      Bypass.stub(bypass, "GET", "/v3.1/region/doesnotexist", fn conn ->
        Conn.resp(conn, 404, "[]")
      end)

      on_exit(fn -> Bypass.down(bypass) end)

      {:ok, endpoint: "http://localhost:#{bypass.port}/v3.1/region/asia"}
    end
  end

  test "fetches Asia countries and validates structure", %{endpoint: endpoint} do
    {:ok, resp} = HTTPoison.get(endpoint, [], recv_timeout: 8000)
    assert resp.status_code == 200

    {:ok, countries} = Jason.decode(resp.body)
    assert is_list(countries)
    assert Enum.count(countries) > 30

    for c <- countries do
      assert Map.has_key?(c, "name")
      assert Map.has_key?(c, "region")
      assert Map.has_key?(c, "area")
      assert c["region"] == "Asia"
      assert is_float(c["area"]) or is_integer(c["area"])
    end
    TestReportGenerator.append_result(%{
      name: "fetches Asia countries and validates structure",
      status: "passed",
      message: "All countries validated"
    })
  end

  property "country area is non-negative and reasonable", %{endpoint: endpoint} do
    {:ok, resp} = HTTPoison.get(endpoint, [], recv_timeout: 8000)
    {:ok, countries} = Jason.decode(resp.body)
    check all country <- StreamData.member_of(countries) do
      area = country["area"]
      assert area >= 0 and area < 20000000
    end
    TestReportGenerator.append_result(%{
      name: "country area is non-negative and reasonable",
      status: "passed",
      message: "All country areas valid"
    })
  end

  test "handles API error gracefully", %{endpoint: endpoint} do
    error_endpoint = String.replace(endpoint, "asia", "doesnotexist")
    {:ok, resp} = HTTPoison.get(error_endpoint, [], recv_timeout: 8000)
    assert resp.status_code in [404, 400]
    TestReportGenerator.append_result(%{
      name: "handles API error gracefully",
      status: "passed",
      message: "Error handled: status #{resp.status_code}"
    })
  end

  test "invalid json is caught" do
    case Jason.decode("{not json}") do
      {:error, _} -> assert true
      _ -> flunk("Should not parse invalid json")
    end
    TestReportGenerator.append_result(%{
      name: "invalid json is caught",
      status: "passed",
      message: "Invalid JSON correctly detected"
    })
  end

end
