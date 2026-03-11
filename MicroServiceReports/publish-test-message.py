#!/usr/bin/env python3
"""
Script para publicar mensajes de prueba a RabbitMQ para el MicroServiceReports
Requiere: pip install pika
"""

import pika
import json
import sys
from datetime import datetime

def publish_test_sale(sale_id=123):
    # Conectar a RabbitMQ
    credentials = pika.PlainCredentials('guest', 'guest')
    parameters = pika.ConnectionParameters('localhost', 5672, '/', credentials)
    
    connection = pika.BlockingConnection(parameters)
    channel = connection.channel()
    
    # Declarar exchange
    channel.exchange_declare(exchange='ventas.events', exchange_type='topic', durable=True)
    
    # Crear mensaje de prueba
    message = {
        "saleId": sale_id,
        "date": datetime.utcnow().isoformat() + "Z",
        "user": "admin",
        "ci": "12345678",
        "client": "Juan Pérez",
        "total": 350.00,
        "products": [
            {
                "productId": 1,
                "name": "Don Quijote de la Mancha",
                "quantity": 2,
                "unitPrice": 120.50
            },
            {
                "productId": 2,
                "name": "Cien Años de Soledad",
                "quantity": 1,
                "unitPrice": 109.75
            }
        ]
    }
    
    # Publicar mensaje
    channel.basic_publish(
        exchange='ventas.events',
        routing_key='venta.confirmada',
        body=json.dumps(message, ensure_ascii=False),
        properties=pika.BasicProperties(
            delivery_mode=2,  # Mensaje persistente
            content_type='application/json'
        )
    )
    
    print(f"✅ Mensaje publicado exitosamente:")
    print(json.dumps(message, indent=2, ensure_ascii=False))
    
    connection.close()

if __name__ == "__main__":
    sale_id = int(sys.argv[1]) if len(sys.argv) > 1 else 123
    
    try:
        publish_test_sale(sale_id)
    except Exception as e:
        print(f"❌ Error: {e}")
        print("\nAsegúrate de que RabbitMQ esté corriendo:")
        print("  brew services start rabbitmq")
        sys.exit(1)
