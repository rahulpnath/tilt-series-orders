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

```sh
tilt up          # from this folder — Orders on its own
```

`project_name='shop-orders'` keeps this a separate compose project even when the
workspace Tiltfile loads all four repos at once, and `new_name=` on `sqlserver`
and `rabbitmq` avoids colliding with Catalog's services of the same name — Tilt
resource names are global, compose service names are not.
`main/Shop.Catalog/README.md` has the long version of both.

`v4-start` did not change this repo — its tag points at the same commit as
`v3-end`.

---

## Earlier: `v3-start` and `v3-end`

| | |
|---|---|
| `Shop.Orders.Api` | minimal API on :5102 — saves orders to SQL Server, publishes `OrderPlaced` |
| `Shop.Orders.Worker` | handles `OrderPlaced` and logs a receipt |
| `Shop.Orders.Contracts` | the message type both sides share |

Same shape as `Shop.Catalog`, deliberately: its own repo, its own
`docker/compose.yaml`, its own SQL Server and RabbitMQ, its own project name.

```sh
docker compose -f docker/compose.yaml up
curl http://localhost:5102/orders
```

### It can see Catalog now

`orders-api` joins `shopnet`, the external network Catalog also joins, so the
name finally resolves:

```sh
docker network create shopnet          # once, outside any project

curl http://localhost:5102/catalog-check
# {"reached":"http://catalog-api:5101","ok":true,"products":"[...]"}
```

```sh
docker exec <orders-api> getent hosts catalog-api
# 172.21.0.2   catalog-api
```

`getent`, not `ping` — most images don't ship `ping`.

Note the `networks:` list on `orders-api` includes `default` as well as
`shopnet`. Naming any network at all turns off the automatic one, and without
`default` this container would lose its own SQL Server and RabbitMQ.

---

### Earlier: what `v3-start` looked like

`orders-api` wants to call `catalog-api`, server-side, by name:

```sh
curl http://localhost:5102/catalog-check
```

```json
{
  "reached": "http://catalog-api:5101",
  "ok": false,
  "error": "Name or service not known (catalog-api:5101)"
}
```

Catalog is running. It's just running in a **different compose project**, which
means a different network, and Docker's DNS is per network. Nothing is broken —
these two were never introduced.

That was the wall Video 3 opens on.

### Why this repo publishes fewer ports than Catalog

`orders-api` publishes 5102 because the Storefront calls it **from the browser**,
and the browser resolves names on your machine rather than on any Docker network.

SQL Server publishes nothing. RabbitMQ publishes nothing either — Catalog's
compose file already binds 15672 on your machine, and two containers can't share
a host port. Run both projects and you'll meet that the first time.
