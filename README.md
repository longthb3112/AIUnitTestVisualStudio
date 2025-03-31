### AI Unit Test Extension for Visual Studio
AI Unit Test Extension is a powerful Visual Studio plugin designed to streamline unit test generation for C# developers. With AI-driven automation, this extension allows you to instantly generate unit tests for any selected .cs file, saving time and effort in writing test cases manually.

**Key Features:**
* **AI-Powered Unit Test Generation** – Automatically generate unit tests for selected C# files using AI.
* **Support for OpenAI and Gemini** – Choose between OpenAI and Gemini models to generate unit tests, allowing you to select the best results based on your needs.
* **Customizable Prompts** – Tailor the AI’s behavior by modifying prompts to generate test cases that align with your specific requirements.
* **Flexible Output Options** – Choose a preferred output folder for saving the generated test files, ensuring better project organization.
* **Persistent Configuration** – The plugin automatically saves user preferences, such as custom prompts and output folder paths, and reloads them on the next use.
* **Seamless Integration** – Works directly within Visual Studio, enhancing your development workflow without additional setup.

**Why Use AI Unit Test Extension?**
- With AI Unit Test Extension, writing unit tests has never been easier—generate, customize, and save your test cases effortlessly! By supporting both OpenAI and Gemini, the plugin enables users to compare results from different AI models and choose the best one for their specific use case.

**Important Notes:**
- The default prompt utilizes xUnit and Moq for generating unit tests. However, you have the flexibility to modify both the prompt and the AI model to better suit your requirements.
- This plugin supports two AI models: OpenAI and Gemini. Users can switch between them and select the best results based on their preference.
- Feel free to experiment with different configurations and provide feedback to help improve the tool’s performance.
- When multiple classes need to be mocked, the generated test cases may require adjustments to align with your specific needs.
- To ensure optimal unit test generation, the maximum supported method length is 150 lines.
- This plugin exclusively supports C# files (.cs extension).
   
## Getting Started

* You can install this [plugin AI Unit VS plugin](https://github.com/longthb3112/AIUnitTestVisualStudio/blob/main/AIUnittestExtension.vsix). 
* Installed plugin is under Tools -> Generate Unit Test For Selected File
- ![Menu](https://github.com/longthb3112/AIUnitTestVisualStudio/blob/main/Plugin-Menu.png)
* You can select .cs file that need to generate Unit Test and input all required information then click **Generate Unit Test** button
- ![UI](https://github.com/longthb3112/AIUnitTestVisualStudio/blob/main/Plugin_View.png)
* When Unit Test file is generated, it will show a confirm message with path of generated file and copy the path to your clipboard.

## How to get key: 
- Gemini api key  [instruction](https://www.merge.dev/blog/gemini-api-key)
- ChatGPT api key [instruction](https://www.merge.dev/blog/chatgpt-api-key)

### Prerequisites

* .Net Framework 4.8
* Visual Studio


### Sample Input class and Output class
There is a [Sample Test Project](https://github.com/longthb3112/AIUnitTestVisualStudio/tree/main/AIUnittestExtension/SampleTestProject) in this repository.
I used this plugin to generate Unit Test.
- Input: [Sample](https://github.com/longthb3112/AIUnitTestVisualStudio/blob/main/AIUnittestExtension/SampleTestProject/Sample.cs)
```
public int Sum(int a, int b)
{
    if (_test == null) throw new ArgumentNullException();

    int c = _test.PlusOne(a);
    return a + b + c;
}
```
- Output: [Sample](https://github.com/longthb3112/AIUnitTestVisualStudio/blob/main/AIUnittestExtension/SampleTestProject/SampleTest.cs)
```
 [Fact]
 public void Sum_ValidInputs_ReturnsCorrectSum()
 {
     // Arrange
     var mockTest = new Mock<ITest>();
     mockTest.Setup(m => m.PlusOne(It.IsAny<int>())).Returns(1);
     var sample = new Sample(mockTest.Object);
     int a = 2;
     int b = 3;
     // Act
     var result = sample.Sum(a, b);
     // Assert
     Assert.Equal(6, result); // 2 + 3 + 1 = 6
 }

 [Fact]
 public void Sum_NullTest_ThrowsArgumentNullException()
 {
     // Arrange
     var sample = new Sample(null);
     int a = 2;
     int b = 3;
     // Act & Assert
     var exception = Assert.Throws<ArgumentNullException>(() => sample.Sum(a, b));
     Assert.NotNull(exception);
 }

 [Fact]
 public void Sum_ZeroInputs_ReturnsCorrectSum()
 {
     // Arrange
     var mockTest = new Mock<ITest>();
     mockTest.Setup(m => m.PlusOne(It.IsAny<int>())).Returns(1);
     var sample = new Sample(mockTest.Object);
     int a = 0;
     int b = 0;
     // Act
     var result = sample.Sum(a, b);
     // Assert
     Assert.Equal(1, result); // 0 + 0 + 1 = 1
 }

 [Fact]
 public void Sum_NegativeInputs_ReturnsCorrectSum()
 {
     // Arrange
     var mockTest = new Mock<ITest>();
     mockTest.Setup(m => m.PlusOne(It.IsAny<int>())).Returns(1);
     var sample = new Sample(mockTest.Object);
     int a = -2;
     int b = -3;
     // Act
     var result = sample.Sum(a, b);
     // Assert
     Assert.Equal(-4, result); // -2 + -3 + 1 = -4
 }
```
  
## Built With

* [.Net framework 4.8](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net48)
* [OpenAI](https://openai.com/)
* [Gemini](https://gemini.google.com/app)
* [xUnit](https://xunit.net/)
* [Moq](https://github.com/moq)
* [Extensibility Essentials 2022](https://marketplace.visualstudio.com/items?itemName=MadsKristensen.ExtensibilityEssentials2022)
* [OpenAI](https://platform.openai.com/docs/overview)

## Authors

* **Long Tran**

