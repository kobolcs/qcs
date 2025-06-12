import http from 'k6/http';
import { check, sleep } from 'k6';
import { htmlReport } from "https://raw.githubusercontent.com/benc-uk/k6-reporter/main/dist/bundle.js";

export const options = {
  thresholds: {
    'http_req_duration': ['p(95)<500'], // 95% of requests must complete below 500ms
    'http_req_failed': ['rate<0.01'],   // The error rate must be less than 1%
  },
  stages: [
    { duration: '5s', target: 5 },
    { duration: '10s', target: 5 },
    { duration: '5s', target: 0 },
  ],
};

export default function () {
  const BASE_URL = 'https://restful-booker.herokuapp.com';
  const res = http.get(`${BASE_URL}/booking`);
  check(res, {
    'status is 200': (r) => r.status === 200,
  });
  sleep(1);
}

// This function is called at the end of the test
export function handleSummary(data) {
  return {
    "summary.html": htmlReport(data),
  };
}