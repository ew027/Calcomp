# Calcomp
This is a .NET library to read Calcomp 907 plot files. Calcomp made electromechanical pen-based plotters that were popular in the 1960s, 70s and 80s; the company carried on making them up until the late 90s when it shut down. The 907 plot format was a way of storing plot information that could be sent directly to Calcomp plotters - on a DOS-based PC with a plotter attached to a port, printing a 907 plot was simply a matter of outputting the contents of the plot to the relevant port. There's unfortunately not much documentation available about the 907 plot file format and this library has been built using bits and pieces of information that I've been able to track down from various sources. As a consequence, it is very much a work in progress and although I've implemented sufficient functionality to be able to fully read the plot files I was interested in viewing, there's other features available in the file format that I'm unable to handle at the moment due to a lack of sufficient information.

### Some sources of information

The University of Edinburgh has a computing history archive which contains some Fortran (77?) code for the VAX 11 dating back to 1982 that would have been used to create plots in the 907 format. Reading through this code eventually allowed me to decode the file header, plot deltas and pen commands. There are other commands in this Fortran code that I should be able to decode given enough time. (http://history.dcs.ed.ac.uk/archive/apps/edwin/calcomp/vax.for)

Some Calcomp plotter manuals are archived in the Bitsavers archive, e.g. (http://bitsavers.trailing-edge.com/pdf/calcomp/CalComp_Software_Reference_Manual_Oct76.pdf). This manual from 1976 doesn't specifically cover the 907 format but there enough similarities to give a rough idea of what the format involved.

## Getting started

The solution should build on Visual Studio 2013 and later. To use the console application to convert a plot file:

    Calcomp2Png input.plt output.png [scalefactor] [-instructions]

The scalefactor argument is optional and defaults to 1, valid values are any positive decimal value (although depending on the size of your plot, large scale factors may produce extremely large images and memory capacity might be an issue). Specifying the -instructions argument will create a text file with the same name as the PNG file containing the plot instructions it was able to decode from the plot file.

The solution contains a testing project with a number of unit tests focusing on the delta object. This will be expanded as I add more functionality to the library.



