# Shop.Orders

The orders context of the demo shop, used by the
**[Local Dev from First Principles](https://github.com/rahulpnath/tilt-series)**
series (Docker, Tilt & Caddy).

Don't clone this repo on its own — clone the workspace repo and let it assemble
the right set of repos at the right tag:

```sh
git clone https://github.com/rahulpnath/tilt-series
cd tilt-series
./bootstrap.sh v3-start
```

## At `v3-start`

The second app repo, in the same shape as `Shop.Catalog`: an API on `:5102`, a worker, a
`Contracts` project for the message they share. `POST /orders` asks Catalog for the product,
saves the order with its name and price, and publishes `OrderPlaced`; the worker logs it.

There is no SQL Server or RabbitMQ in `compose.yaml` — both come from the workspace's
**`infra/`** project, on a network named `shopnet` that the API and worker join. Catalog's
address is set the same way, by name:

```yaml
Catalog__Url: "http://catalog-api:5101"
```

At this tag that name does not resolve. Video 3 is about why, and what to call things instead.

### Running it

Infra first, from the workspace root, then Catalog, then this:

```sh
(cd infra && docker compose up -d --wait)
```

```sh
echo 'SA_PASSWORD=P@ssw0rd!Demo1' > .env     # not committed; Compose reads it
docker compose up
```

```sh
curl -X POST http://localhost:5102/orders \
  -H 'Content-Type: application/json' \
  -d '{"productId":1,"quantity":2}'
```

```
System.Net.Http.HttpRequestException: Name or service not known (catalog-api:5101)
```

```
Shop.Orders/
├── compose.yaml                  # api + worker, on `default` and `shopnet`
├── .env                          # you create this; gitignored
├── orders-api.dev.Dockerfile     # sdk:10.0 + watch
├── orders-worker.dev.Dockerfile
├── .dockerignore
└── src/
    ├── Shop.Orders.Api/          # EF Core, Rebus publish, HttpClient to Catalog
    ├── Shop.Orders.Contracts/    # the OrderPlaced message
    └── Shop.Orders.Worker/       # subscribes and logs
```
