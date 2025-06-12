FROM golang:1.21-alpine AS builder
WORKDIR /app
COPY . .
RUN go test -c -o app.test

FROM alpine
WORKDIR /app
COPY --from=builder /app/app.test .
COPY . .
ENV OWM_API_KEY=
ENTRYPOINT ["./app.test", "-test.v"]
