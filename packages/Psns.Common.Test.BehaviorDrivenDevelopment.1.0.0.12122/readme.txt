* Behavior Driven Development is a testing pattern that builds on Test Driven Development principles.
	(see http://en.wikipedia.org/wiki/Behavior-driven_development)

* The methods provided by the BehaviorDrivenDevelopmentBase use the Arrange-Act-Assert pattern.
	(see http://c2.com/cgi/wiki?ArrangeActAssert)

* The idea is to structure your tests in a way that tells a story that reads like a complete sentence.

See the unit test for the BehaviorDrivenDevelopmentBase class for an example:
	https://tfs.psns.sy/tfs/PSNS/PSNS.Common/_versionControl#path=%24%2FPSNS.Common%2F.Net%2FTest%2FBehaviorDrivenDevelopment%2FMain%2Ftest%2FBDD.UnitTests%2FWhenWorkingWithBDDBase.cs&_a=contents

This test tells the story: "When Working With BDDBase, Then Arrange And Act Should Have Been Called."

* Use inheritance to tell a longer story.

Example:

public class WhenWorkingWithAUser : BehaviorDrivenDevelopmentBase 
{
	public override void Arrange()
    {
        base.Arrange();

        // set up a valid user
    }

    public override void Act()
    {
        base.Act();

        // act on user in test
    }  
}

/** MSTest requires that the class containing the method 
	using the Assert object be decorated with
	the attributes like the classes below **/

[TestClass]
public class AndTheUserIsValid : WhenWorkingWithAUser
{
	[TestMethod]
	public void ThenTheUserShouldBeValid()
	{
		// Make Assertion Here
	}
}

[TestClass]
public class AndTheUserIsInvalid : WhenWorkingWithAUser
{
	public override void Arrange()
    {
        base.Arrange();

        // set up an invalid user
    }

	[TestMethod]
	public void ThenTheUserMustBeInvalid()
	{
		// Make Assertion Here
	}
}