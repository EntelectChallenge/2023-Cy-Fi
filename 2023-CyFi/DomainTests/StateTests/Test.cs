using Domain.Components;
using NUnit.Framework;
using PropertyChanged.SourceGenerator;

namespace DomainTests.StateTests;

internal partial class TestState : State
{
    [Notify] private string testString;
    [Notify] private string anotherTest;
    [Notify] private int intTest;
    [Notify] private object objectTest;
}

public class Test
{
    [SetUp]
    public void ResetIdGenerator()
    {
        // Clean up between runs to deal with static variables being weird
        State.latestId = 1;
        Tracker.StateChanges.Clear();
    }
    
    [Test]
    public void WhenCreated_ThenGenerateCreationChange()
    {
        var testState = new TestState {TestString = "test"};
        testState.AnotherTest = "Ha Got 'Em";
        var changes = Tracker.SerializeStateChanges();
        Console.WriteLine(changes);
    }

    [Test]
    public void WhenGivenStateChanges_DeserializeStateChanges()
    {
        const string serializedState = "START\n" +
                                       "Created StateTracker.TestState - ID: 1\n" +
                                       "Updated TestString = test - ID: 1\n" +
                                       "Updated AnotherTest = Ha Got 'Em - ID: 1";

        var stateChanges = Applier.DeserializeStateChanges(serializedState);
        Tracker.StateChanges = stateChanges;
        var generatedOutput = Tracker.SerializeStateChanges();
        
        Assert.AreEqual(serializedState, generatedOutput);
    }

    [Test]
    public void WhenGivenStateChanges_DeserializeStateChanges_AndApplyChangesToState()
    {
        const string serializedState = "START\n" +
                                       "Created DomainTests.StateTests.TestState - ID: 1\n" +
                                       "Updated TestString = test - ID: 1\n" +
                                       "Updated AnotherTest = Ha Got 'Em - ID: 1";

        StateManager stateManager = new();
        Applier.ApplyChanges(stateManager, serializedState);

        Assert.IsTrue(stateManager.StateDict.Count == 1);
        var testObject = stateManager.StateDict.Values.ToList()[0];
        Assert.IsTrue(testObject.Id == 1); 
        var testState = testObject as TestState;
        Assert.IsTrue(testState?.TestString == "test");
        Assert.IsTrue(testState?.AnotherTest == "Ha Got 'Em");
    }

    [Test]
    public void test_int_serialization()
    {
        var testState = new TestState();
        testState.IntTest = 1;
        testState.ObjectTest = new {testproperty = "test property"};
        var changes = Tracker.SerializeStateChanges();
        Console.WriteLine(changes);
    }
    
    [Test]
    public void test_int_deserialization()
    {
        const string input = "START\n"
                             + "Created DomainTests.StateTests.TestState - ID: 1\n"
                             + "Updated IntTest = 1 - ID: 1";
        var manager = new StateManager();
        Applier.ApplyChanges(manager, input);
        var testObject = (TestState) manager.StateDict.Values.ToList()[0];
        Assert.AreEqual(1, testObject.IntTest);
    }
    
    [Test]
    public void test_string_serialization()
    {
        var testState = new TestState();
        testState.TestString = "test value";
        var changes = Tracker.SerializeStateChanges();

        var objectId = testState.Id;
        
        var expected = "START\n"
                             + $"Created DomainTests.StateTests.TestState - ID: {objectId}\n"
                             + $"Updated TestString = test value - ID: {objectId}";

        Assert.AreEqual(expected, changes);
        
        Console.WriteLine(changes);
    }
    
    [Test]
    public void test_string_deserialization()
    {
        const string input = "START\n"
                             + "Created DomainTests.StateTests.TestState - ID: 1\n"
                             + "Updated TestString = test value - ID: 1\n";
        var manager = new StateManager();
        Applier.ApplyChanges(manager, input);
        var testObject = (TestState) manager.StateDict.Values.ToList()[0];
        Assert.AreEqual("test value", testObject.TestString);
    }
    
    [Test]
    public void test_object_serialization()
    {
        var testState = new TestState();
        testState.ObjectTest = new {testproperty = "test property"};
        var changes = Tracker.SerializeStateChanges();
        Console.WriteLine(changes);
    }
    
    [Test]
    public void test_object_deserialization()
    {
        const string input = "START\n"
                             + "Created DomainTests.StateTests.TestState - ID: 1\n"
                             + "Updated ObjectTest = { testproperty = test property } - ID: 1\n";
        var manager = new StateManager();
        Applier.ApplyChanges(manager, input);
        var state = manager.StateDict;
    }
}