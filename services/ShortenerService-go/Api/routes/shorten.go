package routes

import (
	"time"
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

	body := new(request)

	// like var result = .. in C#
	if err := c.BodyParser(&body); err!= nil {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map{"error":"cannot parse JSON"})
	}

	// implement rate limiting

	// check if the input is an actual URL
	if !govalidator.IsURL(body.URL) {
		return c.Status(fiber.StatusBadRequest).JSON(fiber.Map("error","Invalid URL"))
	}

	// check for domain error
	if !helper.RemoveDomainError(body.URL){
		return c.Status(fiber.StatusServiceUnavilble).JSON(fiber.Map("error","Unavilble"))
	}

	// enforce https, ssl
	body.URL = helpers.EnforeceHTTP(body.URL)
}
