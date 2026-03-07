package router

import (
	"github.com/go-chi/chi/v5"
	"github.com/go-chi/cors"

	"github.com/gauthier-se/GreenSkills-API/internal/handler"
	"github.com/gauthier-se/GreenSkills-API/internal/middleware"
)

// Config holds the dependencies needed to build the router.
type Config struct {
	CORSAllowedOrigins []string
}

// New creates and returns a fully configured chi router with all middleware
// and routes mounted.
func New(cfg Config) *chi.Mux {
	r := chi.NewRouter()

	// Global middleware stack
	r.Use(middleware.Recovery)
	r.Use(middleware.Logger)
	r.Use(cors.Handler(cors.Options{
		AllowedOrigins:   cfg.CORSAllowedOrigins,
		AllowedMethods:   []string{"GET", "POST", "PUT", "DELETE", "OPTIONS"},
		AllowedHeaders:   []string{"Accept", "Authorization", "Content-Type"},
		ExposedHeaders:   []string{"Link"},
		AllowCredentials: false,
		MaxAge:           300,
	}))

	// API routes
	r.Route("/api", func(r chi.Router) {
		r.Use(middleware.JSON)

		r.Get("/health", handler.Health())
	})

	return r
}
