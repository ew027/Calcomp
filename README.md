# Calcomp
This is a .NET library to read Calcomp 907 plot files. Calcomp made pen-based plotters up until the early 90s when the company disappeared. The 907 plot format was a way of storing plot information that could be sent directly to Calcomp plotters - on a DOS-based PC with a plotter attached to a port, printing a 907 plot was simply a matter of outputting the contents of the plot to the relevant plot. There unfortunately not much documentation available about the 907 plot file format and this library has been built using bits and pieces from various sources. As a consequence, it is very much a work in progress and although I've implemented sufficient functionality to be able to fully read the plot files I was interested in viewing, there's other features available in the file format that I'm unable to handle at the moment due to a lack of sufficient information.

# Getting started

The solution should build on Visual Studio 2013 and later. To use the console application to convert a plot file:

Calcomp2Png input.plt output.png [scalefactor] [-instructions]

The scalefactor argument is optional and defaults to 1, valid values are any positive decimal value (although depending on the size of your plot, large scale factors may produce extremely large images and memory capacity might be an issue). Specifying the -instructions argument will create a text file with the same name as the PNG file containing the plot instructions it was able to decode from the plot file.

The solution contains a testing project with a rnage of unit tests focusing on the delta object. This will be expanded as I add more functionality to the library.



