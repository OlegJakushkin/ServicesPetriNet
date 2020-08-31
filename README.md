# ServicesPetriNet
Coloured Stochastic Petri Net with Gropings and variable Time: "Services Petri Net" - PN engine created in CSharp with C# API for PN description

# What is a model here:
Composition of: Groups, Places, Transitions, MarkTypes and Patterns:
 - The group contains it all: nested Groups, Places, Transitions, MarkTypes. (alike a programme it can be started, simulated step-by-step and stopped)
 - Patterns are used to simplify group creation are used to inject Places, Transitions. (similar to a template - it provides blueprints for nodes organization) 
 - MarkTypes can represent any Passive Data Structure (POD). A user defines his MarkTypes. (like a datatype - it handles data)
 - Transitions operate on MarkTypes - Generate, Consume, Compose and Decompose them. (similar to a code-logic - it governs what shall be taken from where and where to put results) 
 - Actions describe the rules of what happens to MarkTypes when Transitions get activated. To function, each Transition has an Action bound to it. (similar to service - it implements logic)
 - Places contain instances of MarkTypes. Transitions take MarkTypes from Places and placemarks into them. (similar to a container it stores and provides access to data)
 - Petri Nets operate on directed bipartite graphs composed mainly of Places and Transitions. 

A simple model example: 
```csharp
using System;
using System.IO;
using System.Linq;
using ServicesPetriNet.Core;
using ServicesPetriNetCore.Core.Simulation.Draw;

namespace ServicesPetriNet {
    public class SimpleDemoProgram {
        public class Mark : MarkType {
            public int value;
        }

        public class Add : ActionBase {
            public static Mark Action(Mark fromA, Mark fromB) {
                return new Mark {
                    value = fromA.value + fromB.value
                };
            }
        }

        public class Sample: Group {
            public Place A, B, C;

            private Transition Summ;

            public Sample() {
                Summ.Action<Add>()
                    .In<Mark>(A)
                    .In<Mark>(B)
                    .Out<Mark>(C);

                Marks = Extensions.At(A, MarkType.Create<Mark>(5))
                    .At(B, MarkType.Create<Mark>(6));
            }
        }

        public static void Main() {
            var simulation = new SimulationController<Sample>();

            var s = simulation.DebugGraphToDot();
            Console.Write(s);
            File.WriteAllText("./simple.dot", s);

            simulation.SimulationStep();
            var result = simulation.TopGroup.C.GetMarks().First() as Mark;
            Assert.AreEqual(5 + 6, result.value);
            Console.Write("Simulation completed!");

            Console.ReadLine();
        }
    }
}

```
Let's discuss what this does from a code standpoint:
 - We declare a POD MarkType `Mark`. Its instances can hold an integer value
 - We declare an `Add` Action that can operate on marks - take two in and return one out. 
   - Note that no interface limits what arguments Action function takes in and what it returns. 
 - We declare a Group with 3 Places `A, B, C` and one Transition `Summ`. 
   - In its constructor, we create a graph of how our transitions will work. 
   - We connect inputs: places `A` and `B` with output place `C`. 
   - We state that our `Summ` transition will use `Add` Action class to perform Transition on MarkType `Mark` instances related to those places: Ins and Outs.
- In main we
  - Create a simulation of the `Sample` Group
  - DebudDraw/Save Group topology to .dot GraphViz file
  - simulate a one step 
  - check results

 So we get a graph like [this](https://bit.ly/2QAbLxQ): 
 
<img src="https://user-images.githubusercontent.com/2915361/91677982-ffceb480-eb4c-11ea-98b8-ec658e88763a.png" width="300" >

*Note*:
  - There are no marks shown! This is a feature you can implement=)
  - There is a DecomposedMarks place provided with every group for Decomposed marks storage (it is an extension used for debugging purposes)

Samples:
 - [UnitTests](https://github.com/OlegJakushkin/ServicesPetriNet/tree/master/NUnitTestSPNCore/Tests) are a good base to start reading different ways of how one can declare Group/Action/Pattern, How simulation class can be used to save and restored from file 
 - `GustafsonLawDemoProgram` that shows how one can implement  Gustafson's Law via Petri Nets and generate an image like this:
 
![AmdahlSpeedUp](https://user-images.githubusercontent.com/2915361/91676471-92b92000-eb48-11ea-8330-319a268abcc2.png)

 - `AmdahlLawDemoProgram` that shows how one can implement Amdahl's Law and generate an image like this:
 
  ![image](https://user-images.githubusercontent.com/2915361/91676555-e592d780-eb48-11ea-8828-ff257ad68bdf.png)
  
 - FatTreeDemoProgram (in progress) that shows how one can implement complex MarkTypes routing over a network of places.
 
