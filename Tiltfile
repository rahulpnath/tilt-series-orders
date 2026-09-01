# Orders on its own, or included by the workspace Tiltfile. Its own compose
# project either way, which is why compose.yaml needed no changes.
docker_compose(['docker/compose.yaml'], project_name='shop-orders')

dc_resource('orders-api', labels=['orders'], links=['http://localhost:5102/orders'])
dc_resource('orders-worker', labels=['orders'])

# Renamed: resource names are global to Tilt, and Catalog has these two as well.
dc_resource('sqlserver', new_name='orders-sqlserver', labels=['orders'])
dc_resource('rabbitmq', new_name='orders-rabbitmq', labels=['orders'])
