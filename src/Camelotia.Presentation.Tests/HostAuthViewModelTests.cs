using System;
using Camelotia.Presentation.ViewModels;
using Camelotia.Services.Interfaces;
using FluentAssertions;
using Microsoft.Reactive.Testing;
using NSubstitute;
using Xunit;

namespace Camelotia.Presentation.Tests
{
    public sealed class HostAuthViewModelTests
    {
        private readonly IProvider _provider = Substitute.For<IProvider>();
        private readonly TestScheduler _scheduler = new TestScheduler();
        
        [Fact]
        public void LoginCommandShouldStayDisabledUntilInputIsValid()
        {
            var model = BuildHostAuthViewModel();
            model.Login.CanExecute(null).Should().BeFalse();
            model.Address = "10.10.10.10";
            model.Username = "hello";
            model.Password = "world";
            model.Port = "5000";
            model.Login.CanExecute(null).Should().BeTrue();
        }

        [Fact]
        public void HasErrorsShouldTriggerWhenProviderBreaks()
        {
            _provider
                .HostAuth("10.10.10.10", 5000, "hello", "world")
                .Returns(x => throw new Exception("example"));
                
            var model = BuildHostAuthViewModel();
            model.HasErrors.Should().BeFalse();
                
            model.Port = "5000";
            model.Username = "hello";
            model.Password = "world";
            model.Address = "10.10.10.10";
            model.Login.Execute(null);
                
            _scheduler.AdvanceBy(2);
            model.HasErrors.Should().BeTrue();
            model.ErrorMessage.Should().Be("example");
        }

        [Fact]
        public void ShouldBeBusyWhenLoggingIn()
        {
            var model = BuildHostAuthViewModel();
            model.IsBusy.Should().BeFalse();

            model.Port = "5000";
            model.Username = "hello";
            model.Password = "world";
            model.Address = "10.10.10.10";
            model.Login.Execute(null);
                
            _scheduler.AdvanceBy(2);
            model.IsBusy.Should().BeTrue();
                
            _scheduler.AdvanceBy(2);
            model.IsBusy.Should().BeFalse();
        }

        [Fact]
        public void ShouldTreatNonIntegersAsInvalid()
        {
            var model = BuildHostAuthViewModel();
            model.Login.CanExecute(null).Should().BeFalse();
            
            model.Port = "5000";
            model.Username = "hello";
            model.Password = "world";
            model.Address = "10.10.10.10";
            model.Login.CanExecute(null).Should().BeTrue();

            model.Port = "abc";
            model.Login.CanExecute(null).Should().BeFalse();
            
            model.Port = "42";
            model.Login.CanExecute(null).Should().BeTrue();
        }

        private HostAuthViewModel BuildHostAuthViewModel()
        {
            return new HostAuthViewModel(_provider, _scheduler, _scheduler);
        }
    }
}