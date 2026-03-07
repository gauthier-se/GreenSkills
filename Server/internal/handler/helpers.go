package handler

import (
	"encoding/json"
	"log/slog"
	"net/http"
)

// ErrorResponse represents a JSON error response body.
type ErrorResponse struct {
	Error string `json:"error"`
}

// writeJSON encodes v as JSON and writes it to w with the given status code.
func writeJSON(w http.ResponseWriter, status int, v any) {
	w.WriteHeader(status)
	if err := json.NewEncoder(w).Encode(v); err != nil {
		slog.Error("Failed to encode JSON response", "error", err)
	}
}

// writeError writes a JSON error response with the given status code and message.
func writeError(w http.ResponseWriter, status int, message string) {
	writeJSON(w, status, ErrorResponse{Error: message})
}
