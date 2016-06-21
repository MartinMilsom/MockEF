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
