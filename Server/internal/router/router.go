package router

import (
	"time"

	"github.com/go-chi/chi/v5"
	"github.com/go-chi/cors"

	"github.com/gauthier-se/GreenSkills-API/internal/handler"
	"github.com/gauthier-se/GreenSkills-API/internal/middleware"
)

// Config holds the dependencies needed to build the router.
type Config struct {
	CORSAllowedOrigins []string
	AuthStore          handler.AuthStore
	LevelsStore        handler.LevelsStore
	JWTSecret          string
	JWTExpiry          time.Duration
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

		// Public routes
		r.Get("/health", handler.Health())

		// Auth routes (public)
		if cfg.AuthStore != nil {
			auth := handler.NewAuthHandler(cfg.AuthStore, cfg.JWTSecret, cfg.JWTExpiry)
			r.Post("/auth/register", auth.Register())
			r.Post("/auth/login", auth.Login())
		}

		// Level routes (public)
		if cfg.LevelsStore != nil {
			levels := handler.NewLevelsHandler(cfg.LevelsStore)
			r.Get("/levels", levels.ListLevels())
			r.Get("/levels/{id}/exercises", levels.GetLevelExercises())
		}

		// Protected routes (require valid JWT)
		r.Group(func(r chi.Router) {
			r.Use(middleware.Auth(cfg.JWTSecret))

			// Protected endpoints will be added here (TE-176, TE-177)
		})
	})

	return r
}
