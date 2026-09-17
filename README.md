# Shop.Orders

The orders context of the demo shop, used by the
**[Local Dev from First Principles](https://github.com/rahulpnath/tilt-series)**
series (Docker, Tilt & Caddy).

Don't clone this repo on its own — clone the workspace repo and let it assemble
the right set of repos at the right tag:

```sh
git clone https://github.com/rahulpnath/tilt-series
cd tilt-series
./bootstrap.sh v4-end
```

## At `v4-end`

A `Tiltfile`, and nothing else. `compose.yaml` is untouched.

```python
docker_compose('compose.yaml', project_name='shop-orders')

dc_resource('api', new_name='orders-api', labels=['orders'])
dc_resource('worker', new_name='orders-worker', labels=['orders'])
```

`tilt up` from this folder runs Orders on its own — with infra and Catalog already up. From the workspace root, its `Tiltfile`
`include()`s this one, and this repo stays its own compose project either way.

`project_name` is not optional — Tilt does not read the `name:` key in `compose.yaml`.
`new_name` is what Tilt calls each service: resource names are global across every repo the
workspace loads, and `Shop.Catalog` has an `api` and a `worker` too. The service and container names are unchanged.

## Earlier: `v3-end`

The second app repo, in the same shape as `Shop.Catalog`: an API on `:5102`, a worker, a
`Contracts` project for the message they share. `POST /orders` asks Catalog for the product,
saves the order with its name and price, and publishes `OrderPlaced`; the worker logs it.

There is no SQL Server or RabbitMQ in `compose.yaml` — both come from the workspace's
**`infra/`** project, on a network named `shopnet` that the API and worker join. Catalog's
address is set by name:

```yaml
Catalog__Url: "http://catalog-api:5101"
```

That name resolves now — video 3 gave Catalog's API the alias on `shopnet`. This API has one
too, **`orders-api`**, so the two compose files read the same and a name is ready for whatever
calls Orders next. Both APIs also carry a dev-only **CORS policy**, so the Storefront can call
them from the browser.

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
{"id":1,"productId":1,"productName":"Pour Over Kettle","unitPrice":119.00,"quantity":2}
```

```
worker-1  |  🧾 order.placed — #1 Pour Over Kettle x2
```

```
Shop.Orders/
├── compose.yaml                  # api + worker on `default` and `shopnet`; alias `orders-api`
├── .env                          # you create this; gitignored
├── orders-api.dev.Dockerfile     # sdk:10.0 + watch
├── orders-worker.dev.Dockerfile
├── .dockerignore
└── src/
    ├── Shop.Orders.Api/          # EF Core, Rebus publish, HttpClient to Catalog
    ├── Shop.Orders.Contracts/    # the OrderPlaced message
    └── Shop.Orders.Worker/       # subscribes and logs
```
