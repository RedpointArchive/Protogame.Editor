using System;
using Protoinject;

namespace ProtogameUIStylingTest
{
    public interface IEntityFactory : IGenerateFactory
    {
        ExampleEntity CreateExampleEntity(string name);
    }
}
