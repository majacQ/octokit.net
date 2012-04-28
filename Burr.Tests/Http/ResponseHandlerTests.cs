﻿using System;
using System.Threading.Tasks;
using Burr.Http;
using FluentAssertions;
using Moq;
using Xunit;

namespace Burr.Tests.Http
{
    public class ResponseHandlerTests
    {
        public class MockResponseHandler : ResponseHandler
        {
            public MockResponseHandler(IApplication app)
                : base(app)
            {
            }

            protected override void After<T>(Env<T> env)
            {
                AfterWasCalled = true;
            }

            protected override void Before<T>(Env<T> env)
            {
                BeforeWasCalled = true;
            }

            public bool AfterWasCalled { get; private set; }
            public bool BeforeWasCalled { get; private set; }
        }

        public class TheConstructor
        {
            [Fact]
            public void ThrowsForBadArguments()
            {
                Assert.Throws<ArgumentNullException>(() => new MockResponseHandler(null));
            }
        }

        public class TheCallMethod
        {
            [Fact]
            public async Task InvokesBefore()
            {
                var env = new Mock<Env<string>>();
                var app = new Mock<IApplication>();
                var handler = new MockResponseHandler(app.Object);
                app.Setup(x => x.Call(env.Object))
                    .Returns(Task.FromResult(app.Object))
                    .Callback(() =>
                {
                    handler.BeforeWasCalled.Should().BeTrue();
                    handler.AfterWasCalled.Should().BeFalse();
                });

                await handler.Call(env.Object);

                app.Verify(x => x.Call(env.Object));
            }

            [Fact]
            public async Task InvokesAfter()
            {
                var env = new Mock<Env<string>>();
                var app = new Mock<IApplication>();
                app.Setup(x => x.Call(env.Object)).Returns(Task.FromResult(app.Object));
                var handler = new MockResponseHandler(app.Object);

                await handler.Call(env.Object);

                app.Verify(x => x.Call(env.Object));
                handler.AfterWasCalled.Should().BeTrue();
            }
        }
    }
}
