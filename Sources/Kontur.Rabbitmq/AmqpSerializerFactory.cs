﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Kontur.Rabbitmq
{
    internal class AmqpSerializerFactory : IAmqpSerializerFactory
    {
        private readonly IDictionary<string, IAmqpSerializer> serializers;

        public AmqpSerializerFactory(IDictionary<string, IAmqpSerializer> serializers)
        {
            if (serializers == null)
            {
                throw new ArgumentNullException(nameof(serializers), "Serializers cannot be null");
            }

            if (!serializers.Any())
            {
                throw new ArgumentException("Should be provided at least one serializer", nameof(serializers));
            }

            this.serializers = serializers;
        }

        public IAmqpSerializer CreateSerializer(IMessage message)
        {
            var contentType = message.Headers[AmqpPropertyBuilder.ContentType];

            return this.CreateSerializer(contentType);
        }

        public IAmqpSerializer CreateSerializer(string contentType)
        {
            if (this.serializers.TryGetValue(contentType, out var serializer))
            {
                return serializer;
            }

            return this.serializers.First().Value;
        }
    }
}
