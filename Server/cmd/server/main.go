package main

import (
	"context"
	"log/slog"
	"net/http"
	"os"
	"os/signal"
	"syscall"
	"time"

	"github.com/gauthier-se/GreenSkills-API/internal/config"
	"github.com/gauthier-se/GreenSkills-API/internal/database"
	"github.com/gauthier-se/GreenSkills-API/internal/router"
	"github.com/gauthier-se/GreenSkills-API/migrations"
)

func main() {
	// Load configuration
	cfg := config.Load()

	// Validate required config
	if cfg.JWTSecret == "" {
		slog.Warn("JWT_SECRET is not set, authentication will not work")
	}

	// Connect to PostgreSQL
	ctx := context.Background()
	dbPool, err := database.Connect(ctx, cfg.DatabaseURL)
	if err != nil {
		slog.Warn("Failed to connect to database, starting without DB",
			"error", err,
		)
	} else {
		slog.Info("Connected to database")
		defer dbPool.Close()

		// Run database migrations
		if err := database.RunMigrations(dbPool, migrations.FS); err != nil {
			slog.Error("Failed to run migrations", "error", err)
			os.Exit(1)
		}
	}

	// Build router
	r := router.New(router.Config{
		CORSAllowedOrigins: cfg.CORSAllowedOrigins,
	})

	// Create HTTP server
	listenAddr := ":" + cfg.Port
	srv := &http.Server{
		Addr:         listenAddr,
		Handler:      r,
		ReadTimeout:  10 * time.Second,
		WriteTimeout: 10 * time.Second,
		IdleTimeout:  60 * time.Second,
	}

	// Start server in a goroutine
	go func() {
		slog.Info("Server starting",
			"addr", listenAddr,
			"env", cfg.Env,
		)
		if err := srv.ListenAndServe(); err != nil && err != http.ErrServerClosed {
			slog.Error("Server failed to start", "error", err)
			os.Exit(1)
		}
	}()

	// Graceful shutdown on SIGINT / SIGTERM
	quit := make(chan os.Signal, 1)
	signal.Notify(quit, syscall.SIGINT, syscall.SIGTERM)
	<-quit

	slog.Info("Shutting down server...")

	shutdownCtx, cancel := context.WithTimeout(context.Background(), 10*time.Second)
	defer cancel()

	if err := srv.Shutdown(shutdownCtx); err != nil {
		slog.Error("Server forced to shutdown", "error", err)
		os.Exit(1)
	}

	slog.Info("Server stopped gracefully")
}
