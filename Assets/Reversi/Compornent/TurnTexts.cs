using Unity.Entities;
using Unity.Tiny.Core2D;
using Unity.Tiny;

public struct TurnTexts : IComponentData
{
    [EntityWithComponentsAttribute(new[] { typeof(Sprite2D) })]
    public Entity White;
    [EntityWithComponentsAttribute(new[] { typeof(Sprite2D) })]
    public Entity Black;
};
