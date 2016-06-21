# Rhino.MockEF
Library to enable mocking of Entity Framework. Keeps an in-memory store of data.

The library is designed to make testing with Entity Framework easier, by using an abstraction of your DbContext to enable in-memory use of your entity stores.

syntactically the library uses the TestBuilder pattern to set up your context, you must call the .Setup(..) method for each table/entity you will be using in your test. You can also provide optional seed data on setup if you require.

```CSharp
  var context = new ContextBuilder<IMyContext>()
      .Setup(x => x.Authors, new List<Author> { new Author { Id = 1, Name = "bob" }})
      .Setup(x => x.Books)
  .GetContext();
```

Becuase Rhino.MockEF just provides a mock of the context, you can also perform actions in your test later down the line. Such as adding more data, inspecting the Entity to Assert - or stubbing more specialist methods you have going on in your context interface.

```CSharp
  //adding
  context.MyEntity.Add(obj);
  
  //checking
  Assert.AreEqual(1, context.MyEntity.Count());
  
  //stubbing
  context.Stub(s => s.MyWeirdAndWonderfulThing(Arg<object>.Is.Anything)).Returns(null);
```

The library is dependant on Rhino.Mocks currently, and the returned context provided is built via Rhino's .GenerateMock<T>()

View The MockEF.Tests project for example tests.

###Setup

All you need in order to start working with Rhino.MockEF is to implement a way of providing/injecting your mocked version of the context to your code. A great way to do this is to use a factory. Consider the following:

```CSharp
public interface IFactory
{
  IMyContext Create();
}

public class Factory : IFactory
{
  public IMyFactory Create()
  {
    return new MyContext();
  }
}

//class that needs to use the context
public class Example
{
  private IFactory _factory;
  public Example(IFactory factory)
  {
    _factory = factory;
  }
  
  public bool MethodThatUsesContext()
  {
    //Important. Your context interface MUST implement IDisposable. 
    //Firstly because this won't work otherwise, Secondonly because - you should anyway.
    using (var context = _factory.Create())
    {
      //Use context here.
    }
    return true;
  }
}

//When testing you can then stub your factory to return the context you have set up, like so:
var factory = MockRepository.GenerateMock<IFactory>();
factory.Stub(x => x.Create()).Return(context); //context built using Rhino.MockEF.
//then
var result = new Example(factory).MethodThatUsesContext();
Assert.IsTrue(result);


//When running the code not in test, in you DI registrations you can use something like:
container.Register<IFactory, Factory>();
```

voilà!
