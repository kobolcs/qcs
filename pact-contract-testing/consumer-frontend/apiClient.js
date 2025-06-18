import fetch from 'node-fetch';

export class ApiClient {
  constructor(baseUrl) {
    this.baseUrl = baseUrl;
  }

  async getUser(id) {
    const response = await fetch(`${this.baseUrl}/users/${id}`, {
        headers: { Accept: 'application/json' },
    });
    return response.json();
  }
}
