docker_compose('compose.yaml', project_name='shop-orders')

dc_resource('orders-api', labels=['orders'], links=['https://orders.localhost/orders'])
dc_resource('orders-worker', labels=['orders'])
