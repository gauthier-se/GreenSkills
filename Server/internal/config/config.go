package config

import (
	"log/slog"
	"os"
	"strconv"
	"strings"
	"time"

	"github.com/joho/godotenv"
)

// Config holds all configuration values for the application.
type Config struct {
	Port               string
	DatabaseURL        string
	JWTSecret          string
	JWTExpiry          time.Duration
	CORSAllowedOrigins []string
	Env                string
}

// Load reads configuration from environment variables (and .env file in development).
// It applies sensible defaults for local development where appropriate.
func Load() *Config {
	if err := godotenv.Load(); err != nil {
		slog.Info("No .env file found, using environment variables")
	}

	return &Config{
		Port:               getEnv("PORT", "8080"),
		DatabaseURL:        getEnv("DATABASE_URL", "postgres://postgres:postgres@localhost:5432/greenskills?sslmode=disable"),
		JWTSecret:          getEnv("JWT_SECRET", ""),
		JWTExpiry:          getEnvDuration("JWT_EXPIRY_HOURS", 24),
		CORSAllowedOrigins: getEnvSlice("CORS_ALLOWED_ORIGINS", []string{"http://localhost:*"}),
		Env:                getEnv("ENV", "development"),
	}
}

// IsDevelopment returns true when running in development mode.
func (c *Config) IsDevelopment() bool {
	return c.Env == "development"
}

func getEnv(key, fallback string) string {
	if val := os.Getenv(key); val != "" {
		return val
	}
	return fallback
}

func getEnvSlice(key string, fallback []string) []string {
	if val := os.Getenv(key); val != "" {
		parts := strings.Split(val, ",")
		result := make([]string, 0, len(parts))
		for _, p := range parts {
			if trimmed := strings.TrimSpace(p); trimmed != "" {
				result = append(result, trimmed)
			}
		}
		return result
	}
	return fallback
}

func getEnvDuration(key string, fallbackHours int) time.Duration {
	if val := os.Getenv(key); val != "" {
		if hours, err := strconv.Atoi(val); err == nil {
			return time.Duration(hours) * time.Hour
		}
	}
	return time.Duration(fallbackHours) * time.Hour
}
