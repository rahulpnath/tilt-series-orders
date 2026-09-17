docker_compose('compose.yaml', project_name='shop-orders')

dc_resource('api', new_name='orders-api', labels=['orders'], links=['https://orders.localhost/orders'])
dc_resource('worker', new_name='orders-worker', labels=['orders'])
