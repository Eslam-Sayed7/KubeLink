# Build stage
FROM golang:1.23 AS builder
WORKDIR /app
COPY . .
RUN CGO_ENABLED=0 GOOS=linux GOARCH=amd64 go build -o main .

# Run stage
FROM alpine
WORKDIR /app
COPY --from=builder /app/main .
COPY .env .
EXPOSE 3000
CMD ["./main"]
