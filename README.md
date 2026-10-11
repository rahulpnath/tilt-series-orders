# Shop.Orders

The orders context of the demo shop, used by the
**[Local Dev from First Principles](https://github.com/rahulpnath/tilt-series)**
series (Docker, Tilt & Caddy).

Don't clone this repo on its own — clone the workspace repo and let it assemble
the right set of repos at the right tag:

```sh
git clone https://github.com/rahulpnath/tilt-series
cd tilt-series
./bootstrap.sh v6-end
```

## At `v6-end`

Two videos changed this repo.

**Docker Compose Include** (`v5.2-end`) — the services are `orders-api` and `orders-worker`, so the
workspace can `include` this file into one compose project with the other repos. The worker left
`shopnet`; it finds infra on the project's own network. The `Tiltfile` lost `docker_compose()` and
`new_name=`: the workspace loads this compose file through its `include`, and this `Tiltfile`
keeps only the labels and the link.

**Parallel Worktree Environments** (`v6-end`) — the API's `shopnet` alias is
`orders-api-${FOLDER:-main}`, and the Tilt link is `https://orders-<folder>.localhost/orders`.
`FOLDER` is the workspace folder being run, passed on by the workspace `Tiltfile`; it falls back
to `main`. `Catalog__Url` stays `http://catalog-api:5101` — it resolves on the folder's own
network, so each copy's Orders reaches its own Catalog.

The **`feature-pricing`** branch is the series' example feature: it takes 10% off every order.
Check it out as a worktree under the workspace's `feature-pricing/` folder (workspace README).

This repo no longer runs on its own: its database and message broker come from the workspace's
infra, inside each folder's project. Run it from the workspace (`tilt up`, or `tilt up <folder>`).
Every worktree of this repo needs its own `.env` (`SA_PASSWORD`) — git doesn't copy ignored files.

```sh
curl https://orders-main.localhost/orders
```

## Earlier: `v5-end`

`ports:` is gone from `compose.yaml`, and the `Tiltfile` gains `links=['https://orders.localhost/orders']`.
Nothing else changed.

The API is reached at **https://orders.localhost** through **Caddy** — the workspace's `caddy/`
project, a container on `shopnet` that forwards to the alias **`orders-api:5102`**. The alias from video 3 finally has its caller.

Nothing outside Docker needs this API's door any more: the browser goes through Caddy, and
nothing else ever used the port. The CORS policy in `Program.cs` stays — the Storefront is `storefront.localhost` and this
API is `orders.localhost`, different origins, as they will be when deployed.

```sh
curl https://orders.localhost/orders        # after trusting Caddy's root (workspace README), or add -k
```

## Earlier: `v4-end`

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
├── compose.yaml                  # api + worker on `default` and `shopnet`; alias `orders-api`; no ports
├── .env                          # you create this; gitignored
├── orders-api.dev.Dockerfile     # sdk:10.0 + watch
├── orders-worker.dev.Dockerfile
├── .dockerignore
└── src/
    ├── Shop.Orders.Api/          # EF Core, Rebus publish, HttpClient to Catalog
    ├── Shop.Orders.Contracts/    # the OrderPlaced message
    └── Shop.Orders.Worker/       # subscribes and logs
```
