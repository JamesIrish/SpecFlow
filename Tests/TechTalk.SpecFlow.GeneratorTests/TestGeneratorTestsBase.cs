﻿using System;
using System.IO;
using Moq;
using TechTalk.SpecFlow.Configuration;
using TechTalk.SpecFlow.Generator;
using TechTalk.SpecFlow.Generator.CodeDom;
using TechTalk.SpecFlow.Generator.Generation;
using TechTalk.SpecFlow.Generator.Interfaces;
using TechTalk.SpecFlow.Generator.UnitTestConverter;
using TechTalk.SpecFlow.Generator.UnitTestProvider;
using TechTalk.SpecFlow.Parser;

namespace TechTalk.SpecFlow.GeneratorTests
{
    public abstract class TestGeneratorTestsBase
    {
        protected ProjectPlatformSettings net35CSSettings;
        protected ProjectPlatformSettings net35VBSettings;
        protected ProjectSettings net35CSProjectSettings;
        protected ProjectSettings net35VBProjectSettings;
        protected GenerationSettings defaultSettings;
        protected Mock<ITestHeaderWriter> TestHeaderWriterStub;
        protected Mock<ITestUpToDateChecker> TestUpToDateCheckerStub;

        public TestGeneratorTestsBase()
        {
            net35CSSettings = new ProjectPlatformSettings
                                  {
                                      Language = GenerationTargetLanguage.CSharp,
                                      LanguageVersion = new Version("3.0"),
                                      Platform = GenerationTargetPlatform.DotNet,
                                      PlatformVersion = new Version("3.5"),
                                  };
            net35VBSettings = new ProjectPlatformSettings
                                  {
                                      Language = GenerationTargetLanguage.VB,
                                      LanguageVersion = new Version("9.0"),
                                      Platform = GenerationTargetPlatform.DotNet,
                                      PlatformVersion = new Version("3.5"),
                                  };

            net35CSProjectSettings = new ProjectSettings { ProjectFolder = Path.GetTempPath(), ProjectPlatformSettings = net35CSSettings };
            net35VBProjectSettings = new ProjectSettings { ProjectFolder = Path.GetTempPath(), ProjectPlatformSettings = net35VBSettings };
            defaultSettings = new GenerationSettings();

            TestHeaderWriterStub = new Mock<ITestHeaderWriter>();
            TestUpToDateCheckerStub = new Mock<ITestUpToDateChecker>();
        }

        protected FeatureFileInput CreateSimpleValidFeatureFileInput(string projectRelativeFolderPath = null)
        {
            return CreateSimpleFeatureFileInput(@"
Feature: Addition

@mytag
Scenario: Add two numbers
    Given I have entered 50 into the calculator
    And I have entered 70 into the calculator
    When I press add
    Then the result should be 120 on the screen
",
projectRelativeFolderPath);
        }

        protected FeatureFileInput CreateSimpleFeatureFileInput(string featureFileContent, string projectRelativeFolderPath = null)
        {
            const string FeatureFileName = @"Dummy.feature";
            string projectRelativeFilePath = projectRelativeFolderPath == null
                ? FeatureFileName
                : Path.Combine(projectRelativeFolderPath, FeatureFileName);
            return new FeatureFileInput(projectRelativeFilePath) {FeatureFileContent = featureFileContent};
        }

        protected FeatureFileInput CreateSimpleInvalidFeatureFileInput()
        {
            return CreateSimpleFeatureFileInput(@"
Feature: Addition
Scenario: Add two numbers
    Given I have entered 50 into the calculator
    AndXXX the keyword is misspelled
");
        }

        protected TestGenerator CreateTestGenerator()
        {
            return CreateTestGenerator(net35CSProjectSettings);
        }

        protected TestGenerator CreateTestGenerator(ProjectSettings projectSettings)
        {
            Configuration.SpecFlowConfiguration generatorSpecFlowConfiguration = ConfigurationLoader.GetDefault();
            CodeDomHelper codeDomHelper = new CodeDomHelper(CodeDomProviderLanguage.CSharp);
            UnitTestFeatureGenerator unitTestFeatureGenerator = new UnitTestFeatureGenerator(new NUnit3TestGeneratorProvider(codeDomHelper), codeDomHelper, generatorSpecFlowConfiguration, new DecoratorRegistryStub());

            var gherkinParserFactory = new SpecFlowGherkinParserFactory();

            var generatorRegistryStub = new Mock<IFeatureGeneratorRegistry>();
            generatorRegistryStub.Setup(r => r.CreateGenerator(It.IsAny<SpecFlowDocument>())).Returns(unitTestFeatureGenerator);

            return new TestGenerator(generatorSpecFlowConfiguration, projectSettings, TestHeaderWriterStub.Object, TestUpToDateCheckerStub.Object, generatorRegistryStub.Object, codeDomHelper, gherkinParserFactory);
        }

        protected FeatureFileInput CreateScenarioOutlineValidFeatureFileInput(string projectRelativeFolderPath = null)
        {
            return CreateSimpleFeatureFileInput(@"
Feature: Addition
Feature Description

@mytag
Scenario Outline: Add two numbers
Scenario Description
    Given I have entered <first> into the calculator
    And I have entered <second> into the calculator
    When I press add
    Then the result should be <result> on the screen
Examples: ExName
This is desc
    | first | second | result |
    | 10    | 10     | 20     |
    | 50    | 70     | 120    |
",
projectRelativeFolderPath);
        }
    }
}
