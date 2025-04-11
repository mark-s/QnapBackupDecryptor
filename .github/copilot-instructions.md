Write tests using NUnit, FakeItEasy and Shouldly.
Unit tests should follow the Arrange-Act-Assert pattern and use descriptive names to indicate what's being tested.
Tests should be placed in a separate test project and should not be included in the main project.
The test methods should be placed in a separate test class and should not be included in the main class.
The test project should reference the main project and include the necessary NuGet packages for NUnit, FakeItEasy, and Shouldly.
The test project should be named according to the convention of <MainProjectName>.Tests.
The test class should be named according to the convention of <ClassName>Tests.
The test methods should be named according to the convention of <MethodName>_<Condition>_<ExpectedResult>.
