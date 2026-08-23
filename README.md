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

### It cannot see Catalog, and that's the point

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

That's Video 3.

### Why this repo publishes fewer ports than Catalog

`orders-api` publishes 5102 because the Storefront calls it **from the browser**,
and the browser resolves names on your machine rather than on any Docker network.

SQL Server publishes nothing. RabbitMQ publishes nothing either — Catalog's
compose file already binds 15672 on your machine, and two containers can't share
a host port. Run both projects and you'll meet that the first time.
