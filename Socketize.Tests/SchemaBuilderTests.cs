using System;
using System.Collections.Generic;
using Socketize.Core;
using Socketize.Core.Abstractions;
using Socketize.Core.Enums;
using Socketize.Core.Extensions;
using Socketize.Core.Routing;
using Xunit;

namespace Socketize.Tests
{
    // Disable ReSharper warning about documenting methods.
    #pragma warning disable 1591

    // Disable Stylecop warning about documenting methods.
    #pragma warning disable SA1600

    /// <summary>
    /// Tests for <see cref="SchemaBuilder"/> type.
    /// </summary>
    public class SchemaBuilderTests
    {
        private static Action<ConnectionContext> DummyParameterlessDelegateHandler = context => { };

        private static Action<ConnectionContext, object> DummyDelegateHandler = (context, obj) => { };

        [Fact]
        public void SchemaBuilder_WithSingleRouteInsideHub_ShouldReturnValidSchema()
        {
            // Arrange
            var schemaBuilder = SchemaBuilder.Create()
                .Hub("exampleHub", hub => hub
                    .Route<DummyMessageHandler>("exampleRoute"));

            var expected = new[]
            {
                new SchemaItem("exampleHub/exampleRoute", typeof(DummyMessageHandler), default, HandlerInstanceKind.Class),
            };

            // Act
            var resultingItems = schemaBuilder.Build();

            // Assert
            Assert.Equal(resultingItems, expected, new SchemaItemsUniversalComparer());
        }

        [Fact]
        public void SchemaBuilder_WithSingleDelegateRouteInsideHub_ShouldReturnValidSchema()
        {
            // Arrange
            var schemaBuilder = SchemaBuilder.Create()
                .Hub("exampleHub", hub => hub
                    .Route("exampleRoute", DummyParameterlessDelegateHandler));

            var expected = new[]
            {
                new SchemaItem("exampleHub/exampleRoute", DummyParameterlessDelegateHandler.Method, default, HandlerInstanceKind.Delegate),
            };

            // Act
            var resultingItems = schemaBuilder.Build();

            // Assert
            Assert.Equal(resultingItems, expected, new SchemaItemsUniversalComparer());
        }

        [Fact]
        public void SchemaBuilder_WithSingleDelegateWithParametersRouteInsideHub_ShouldReturnValidSchema()
        {
            // Arrange
            var schemaBuilder = SchemaBuilder.Create()
                .Hub("exampleHub", hub => hub
                    .Route("exampleRoute", DummyDelegateHandler));

            var expected = new[]
            {
                new SchemaItem("exampleHub/exampleRoute", DummyDelegateHandler.Method, typeof(object), HandlerInstanceKind.Delegate),
            };

            // Act
            var resultingItems = schemaBuilder.Build();

            // Assert
            Assert.Equal(resultingItems, expected, new SchemaItemsUniversalComparer());
        }

        [Fact]
        public void SchemaBuilder_WithNoRoutesInsideHub_ShouldReturnValidSchema()
        {
            // Arrange
            var schemaBuilder = SchemaBuilder.Create()
                .Hub("exampleHub", hub => hub);

            var expected = Array.Empty<SchemaItem>();

            // Act
            var resultingItems = schemaBuilder.Build();

            // Assert
            Assert.Equal(resultingItems, expected, new SchemaItemsUniversalComparer());
        }

        [Fact]
        public void SchemaBuilder_WithRouteInsideHubInsideOtherHub_ShouldReturnValidSchema()
        {
            // Arrange
            var schemaBuilder = SchemaBuilder.Create()
                .Hub("exampleHub", hub => hub
                    .Hub("innerExampleHub", innerHub => innerHub
                        .Route<DummyMessageHandler>("exampleRoute")));

            var expected = new[]
            {
                new SchemaItem("exampleHub/innerExampleHub/exampleRoute", typeof(DummyMessageHandler), default, HandlerInstanceKind.Class),
            };

            // Act
            var resultingItems = schemaBuilder.Build();

            // Assert
            Assert.Equal(resultingItems, expected, new SchemaItemsUniversalComparer());
        }

        [Fact]
        public void SchemaBuilder_SimpleRouteInsideHubWithSpecialRoutesDefined_ShouldReturnValidSchema()
        {
            // Arrange
            var schemaBuilder = SchemaBuilder.Create()
                .Hub("exampleHub", hub => hub
                    .Route<DummyMessageHandler>("exampleRoute"))
                .OnConnect<DummyMessageHandler>()
                .OnDisconnect<DummyMessageHandler>();

            var expected = new[]
            {
                new SchemaItem("exampleHub/exampleRoute", typeof(DummyMessageHandler), default, HandlerInstanceKind.Class),
                new SchemaItem(SpecialRouteNames.ConnectRoute, typeof(DummyMessageHandler), default, HandlerInstanceKind.Class),
                new SchemaItem(SpecialRouteNames.DisconnectRoute, typeof(DummyMessageHandler), default, HandlerInstanceKind.Class),
            };

            // Act
            var resultingItems = schemaBuilder.Build();

            // Assert
            Assert.Equal(resultingItems, expected, new SchemaItemsUniversalComparer());
        }

        private class DummyMessageHandler : IMessageHandler
        {
            public void Handle(ConnectionContext connectionContext)
            {
            }
        }

        private class SchemaItemsUniversalComparer : IEqualityComparer<SchemaItem>
        {
            public bool Equals(SchemaItem x, SchemaItem y)
            {
                if (ReferenceEquals(x, y))
                {
                    return true;
                }

                if (ReferenceEquals(x, null))
                {
                    return false;
                }

                if (ReferenceEquals(y, null))
                {
                    return false;
                }

                if (x.GetType() != y.GetType())
                {
                    return false;
                }

                return x.Route == y.Route && x.Handler == y.Handler && x.MessageType == y.MessageType;
            }

            public int GetHashCode(SchemaItem obj)
            {
                return HashCode.Combine(obj.Route, obj.Handler, obj.MessageType);
            }
        }
    }
}