package routes

import (
	"ShortenerService/database"
	helper "ShortenerService/helpers"
	"os"
	"strconv"
	"time"

	"github.com/asaskevich/govalidator"
	"github.com/go-redis/redis/v8"
	"github.com/gofiber/fiber/v2"
	"github.com/google/uuid"
)

type Request struct {
	Url         string        `json:"url"`
	CustomShort string        `json:"short"`
	Expiray     time.Duration `json:"expiray"`
}

type Response struct {
	Url             string        `json:"url"`
	CustomShort     string        `json:"short"`
	Expiray         time.Duration `json:"expiray"`
	XRateRemaining  int           `json:"rate-limit"`
	XRateLimitReset time.Duration `json:"rate-limit-reset"`
}

func ShortenURL(c *fiber.Ctx) error {

	body := new(Request)

	// like var result = .. in C#
	if err := c.BodyParser(&body); err != nil {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{"error": "cannot parse JSON"})
	}

	// implement rate limiting

	r2 := database.CreateClient(1)
	defer r2.Close()
	val, err := r2.Get(database.Ctx, c.IP()).Result()

	if err == redis.Nil {

		_ = r2.Set(database.Ctx, c.IP(), os.Getenv("API_QOUTA"), 30*60*time.Second).Err()

	} else {

		valInt, _ := strconv.Atoi(val)
		if valInt <= 0 {
			limit, _ := r2.TTL(database.Ctx, c.IP()).Result()
			return c.Status(fiber.StatusServiceUnavailable).JSON(fiber.Map{
				"error":            "Rate limit exceeded",
				"rate_limit_reset": limit / time.Nanosecond / time.Minute,
			})
		}
	}

	// check if the input is an actual URL
	if !govalidator.IsURL(body.Url) {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{"error": "Invalid URL"})
	}

	// check for domain error
	if !helper.RemoveDomainError(body.Url) {
		return c.Status(fiber.StatusServiceUnavailable).JSON(fiber.Map{"error": "Unavilble"})
	}

	// enforce https, ssl
	body.Url = helper.EnforceHTTP(body.Url)

	var id string
	if body.CustomShort == "" {
		id = uuid.New().String()[:8] // generate a random 8 character ID
	} else {
		id = body.CustomShort
	}

	r := database.CreateClient(0)
	defer r.Close()

	val, _ = r.Get(database.Ctx, id).Result()
	if val != "" {
		return c.Status(fiber.StatusConflict).JSON(fiber.Map{"error": "Short URL already exists"})
	}

	// set the URL with an expiration time
	if body.Expiray == 0 {
		body.Expiray = 24 * time.Hour
	}

	err = r.Set(database.Ctx, id, body.Url, body.Expiray*3600*time.Second).Err()

	if err != nil {
		return c.Status(fiber.StatusInternalServerError).JSON(fiber.Map{"error": "Failed to store URL"})
	}

	resp := Response{
		Url:             body.Url,
		CustomShort:     "",
		Expiray:         body.Expiray,
		XRateRemaining:  10,
		XRateLimitReset: 30, // 30 minutes would renew the rate limit
	}

	r2.Decr(database.Ctx, c.IP())

	val, _ = r2.Get(database.Ctx, c.IP()).Result()
	resp.XRateRemaining, _ = strconv.Atoi(val)

	ttl, _ := r2.TTL(database.Ctx, c.IP()).Result()
	resp.XRateLimitReset = ttl / time.Nanosecond / time.Minute

	resp.CustomShort = os.Getenv("DOMAIN") + "/" + id

	return c.Status(fiber.StatusOK).JSON(resp)
}
