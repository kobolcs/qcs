defmodule RestCountriesAsiaValidatorTest do
  use ExUnit.Case, async: true
  use ExUnitProperties
  alias TestReportGenerator

  @moduletag timeout: 30000

  @endpoint "https://restcountries.com/v3.1/region/asia"

  test "fetches Asia countries and validates structure" do
    {:ok, resp} = HTTPoison.get(@endpoint, [], recv_timeout: 8000)
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

  property "country area is non-negative and reasonable" do
    {:ok, resp} = HTTPoison.get(@endpoint, [], recv_timeout: 8000)
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

  test "handles API error gracefully" do
    {:ok, resp} = HTTPoison.get("https://restcountries.com/v3.1/region/doesnotexist", [], recv_timeout: 8000)
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
