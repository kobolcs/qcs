package main

import (
	"context"
	"fmt"
	"io"
	"net/http"
	"strings"
	"testing"
)

type trackingBody struct{ closed bool }

func (tb *trackingBody) Read(p []byte) (int, error) { return 0, io.EOF }
func (tb *trackingBody) Close() error               { tb.closed = true; return nil }

// retryTransport simulates a server that always fails to trigger retries
type retryTransport struct{ bodies []*trackingBody }

func (t *retryTransport) RoundTrip(req *http.Request) (*http.Response, error) {
	b := &trackingBody{}
	t.bodies = append(t.bodies, b)
	return &http.Response{StatusCode: 500, Status: "500 Internal Server Error", Body: b, Header: make(http.Header)}, nil
}

func TestGetWithRetryClosesBodies(t *testing.T) {
	rt := &retryTransport{}
	client := &http.Client{Transport: rt}

	_, err := GetWithRetry(context.Background(), client, "http://example.com")
	if err == nil {
		t.Fatal("expected error")
	}

	if len(rt.bodies) != maxRetries {
		t.Fatalf("expected %d attempts, got %d", maxRetries, len(rt.bodies))
	}
	for i, b := range rt.bodies[:len(rt.bodies)-1] {
		if !b.closed {
			t.Errorf("body %d not closed", i)
		}
	}
	if rt.bodies[len(rt.bodies)-1].closed {
		t.Error("final body should not be closed")
	}
	rt.bodies[len(rt.bodies)-1].Close()
}

// errorClient returns a response and an error to check both paths
type errorClient struct {
	bodies []*trackingBody
}

func (c *errorClient) Do(req *http.Request) (*http.Response, error) {
	b := &trackingBody{}
	c.bodies = append(c.bodies, b)
	resp := &http.Response{StatusCode: 500, Status: "500 Internal Server Error", Body: b, Header: make(http.Header)}
	return resp, fmt.Errorf("transport failure")
}

func TestGetWithRetryErrorWithResponse(t *testing.T) {
	ec := &errorClient{}

	resp, err := GetWithRetry(context.Background(), ec, "http://example.com")
	if err == nil {
		t.Fatal("expected error")
	}
	if resp == nil {
		t.Fatal("expected response")
	}
	if !strings.Contains(err.Error(), "transport failure") || !strings.Contains(err.Error(), resp.Status) {
		t.Errorf("unexpected error: %v", err)
	}
	if len(ec.bodies) != maxRetries {
		t.Fatalf("expected %d attempts, got %d", maxRetries, len(ec.bodies))
	}
	for i, b := range ec.bodies[:len(ec.bodies)-1] {
		if !b.closed {
			t.Errorf("body %d not closed", i)
		}
	}
	if ec.bodies[len(ec.bodies)-1].closed {
		t.Error("final body should not be closed")
	}
	resp.Body.Close()
}
