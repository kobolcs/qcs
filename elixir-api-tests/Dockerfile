FROM elixir:1.16-alpine
WORKDIR /app
COPY . .
RUN mix local.hex --force && mix deps.get
CMD ["mix", "test"]
