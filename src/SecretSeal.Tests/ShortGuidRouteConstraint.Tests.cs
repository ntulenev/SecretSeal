using FluentAssertions;

using Microsoft.AspNetCore.Routing;

using SecretSeal.Routing;

using Transport;

namespace SecretSeal.Tests;

public sealed class ShortGuidRouteConstraintTests
{
    [Fact(DisplayName = "Match throws when values are null")]
    public void MatchThrowsWhenValuesAreNull()
    {
        // Arrange
        var constraint = new ShortGuidRouteConstraint();

        // Act
        var action = () => constraint.Match(null, null, "id", null!, RouteDirection.IncomingRequest);

        // Assert
        action.Should().Throw<ArgumentNullException>();
    }

    [Fact(DisplayName = "Match returns false when route key is missing")]
    public void MatchReturnsFalseWhenRouteKeyIsMissing()
    {
        // Arrange
        var constraint = new ShortGuidRouteConstraint();
        var values = new RouteValueDictionary();

        // Act
        var result = constraint.Match(null, null, "id", values, RouteDirection.IncomingRequest);

        // Assert
        result.Should().BeFalse();
    }

    [Fact(DisplayName = "Match returns false when route value is null")]
    public void MatchReturnsFalseWhenRouteValueIsNull()
    {
        // Arrange
        var constraint = new ShortGuidRouteConstraint();
        var values = new RouteValueDictionary { ["id"] = null };

        // Act
        var result = constraint.Match(null, null, "id", values, RouteDirection.IncomingRequest);

        // Assert
        result.Should().BeFalse();
    }

    [Fact(DisplayName = "Match returns false when route value is not a ShortGuid")]
    public void MatchReturnsFalseWhenValueIsInvalid()
    {
        // Arrange
        var constraint = new ShortGuidRouteConstraint();
        var values = new RouteValueDictionary { ["id"] = "not-a-short-guid" };

        // Act
        var result = constraint.Match(null, null, "id", values, RouteDirection.IncomingRequest);

        // Assert
        result.Should().BeFalse();
    }

    [Fact(DisplayName = "Match returns true when route value is a ShortGuid string")]
    public void MatchReturnsTrueWhenValueIsValidString()
    {
        // Arrange
        var constraint = new ShortGuidRouteConstraint();
        var shortGuid = ShortGuid.FromGuid(Guid.NewGuid()).ToString();
        var values = new RouteValueDictionary { ["id"] = shortGuid };

        // Act
        var result = constraint.Match(null, null, "id", values, RouteDirection.IncomingRequest);

        // Assert
        result.Should().BeTrue();
    }

    [Fact(DisplayName = "Match returns true when route value is a ShortGuid instance")]
    public void MatchReturnsTrueWhenValueIsShortGuidInstance()
    {
        // Arrange
        var constraint = new ShortGuidRouteConstraint();
        var shortGuid = ShortGuid.FromGuid(Guid.NewGuid());
        var values = new RouteValueDictionary { ["id"] = shortGuid };

        // Act
        var result = constraint.Match(null, null, "id", values, RouteDirection.IncomingRequest);

        // Assert
        result.Should().BeTrue();
    }
}
