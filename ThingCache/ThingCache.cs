using System.Collections.Generic;
using FakeItEasy;
using FluentAssertions;
using NUnit.Framework;

namespace MockFramework
{
    public class ThingCache
    {
        private readonly IDictionary<string, Thing> dictionary = new Dictionary<string, Thing>();
        private readonly IThingService thingService;

        public ThingCache(IThingService thingService)
        {
            this.thingService = thingService;
        }

        public Thing Get(string thingId)
        {
            Thing thing;
            if (dictionary.TryGetValue(thingId, out thing))
                return thing;
            if (thingService.TryRead(thingId, out thing))
            {
                dictionary[thingId] = thing;
                return thing;
            }
            return null;
        }
    }

    public class ThingCache_Should
    {
        private IThingService thingService;
        private ThingCache thingCache;

        private const string thingId1 = "TheDress";
        private Thing thing1 = new Thing(thingId1);

        private const string thingId2 = "CoolBoots";
        private Thing thing2 = new Thing(thingId2);

        [SetUp]
        public void SetUp()
        {
            thingService = A.Fake<IThingService>();
            thingCache = new ThingCache(thingService);
        }

        [Test]
        public void ReturnsNull_WhenIdNotExists()
        {
            Thing thing;
            A.CallTo(() => thingService.TryRead("1", out thing)).Returns(false);

            thingCache.Get("1").Should().BeNull();
        }

        [Test]
        public void ReturnsThing_WhenIdExists()
        {
            A.CallTo(() => thingService.TryRead("1", out thing1)).Returns(true);

            thingCache.Get("1").Should().Be(thing1);
        }

        [Test]
        public void HappenedOnce_WhenCalled()
        {
            A.CallTo(() => thingService.TryRead("1", out thing1)).Returns(true);

            thingCache.Get("1");
            thingCache.Get("1");

            A.CallTo(() => thingService.TryRead("1", out thing1)).MustHaveHappenedOnceExactly();
        }

        [Test]
        public void NotHappened_WhenWrongId()
        {
            A.CallTo(() => thingService.TryRead("1", out thing1)).Returns(true);

            thingCache.Get("1");

            A.CallTo(() => thingService.TryRead("2", out thing1)).MustNotHaveHappened();
        }

        [Test]
        public void ReturnsAllThings_WhenCalled()
        {
            A.CallTo(() => thingService.TryRead("1", out thing1)).Returns(true);
            A.CallTo(() => thingService.TryRead("2", out thing2)).Returns(true);

            thingCache.Get("1").Should().Be(thing1);
            thingCache.Get("2").Should().Be(thing2);
        }

        [Test]
        public void ReturnsEqualThings_WhenCalledTwoTimes()
        {
            A.CallTo(() => thingService.TryRead("1", out thing1)).Returns(true);

            var firstThing = thingCache.Get("1");
            var secondThing = thingCache.Get("1");

            firstThing.Should().BeEquivalentTo(secondThing);
        }

        /** Проверки в тестах
         * Assert.AreEqual(expectedValue, actualValue);
         * actualValue.Should().Be(expectedValue);
         */

        /** Синтаксис AAA
         * Arrange:
         * var fake = A.Fake<ISomeService>();
         * A.CallTo(() => fake.SomeMethod(...)).Returns(true);
         * Assert:
         * var value = "42";
         * A.CallTo(() => fake.TryRead(id, out value)).MustHaveHappened();
         */

        /** Синтаксис out
         * var value = "42";
         * string _;
         * A.CallTo(() => fake.TryRead(id, out _)).Returns(true)
         *     .AssignsOutAndRefParameters(value);
         * A.CallTo(() => fake.TryRead(id, out value)).Returns(true);
         */

        /** Синтаксис Repeat
         * var value = "42";
         * A.CallTo(() => fake.TryRead(id, out value))
         *     .MustHaveHappened(Repeated.Exactly.Twice)
         */
    }
}