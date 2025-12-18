# Repro sample for running Aspire tests with Playwright and Blazor Web

This project is to reproduce the issue in Visual Studio 2022/2026 where debugging tests that use the aspire test host result in a browser being launched for visual studio to attach to where it should not, breaking the ability to debug Playwright tests.

To repro:

1. Download the solution and open in VS 2026 (Version: Insiders [11312.210] at time of writing.).

2. Build the solution

3. Go to test explorer in visual studio and right click to debug the one test you see. It will likely prompt you in the test output that you need to install Playwright browsers by running a powershells script. Follow those instructions if it does and debug again.


What it should do: 

 1. Spin up the Aspire test host
 2. Launch a Playwright browser and navigate to the web app running on the aspire test host
 3. Type some text in a text area
 4. Fin

What it does:

 1. Spin up the Aspire test host
 2. Launch a Playwright browser and navigate to the web app running on the aspire test host
 3. Visual studio then launches a browser itself to debug the blazor web app
 4. This interrupts/breaks the test as the new browser gets focus
 5. Broken

This is slightly different behaviour to what I'm getting in my own project. I'm not sure what the difference is, but I get similar behaivour except on step three where VS launches the debug browser, 
it doesn't resolve to a url successfully and I see an about:blank page and eventually hit an error with the debugger failing to attach.

I'm not sure why mine is a bit different, but the net result is the same, it shouldn't be launching this debug browser as it prevents executing Playwright tests.
