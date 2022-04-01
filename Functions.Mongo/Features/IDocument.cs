namespace Functions.Mongo.Features;

public interface IDocument<out TId>
{
    TId Id { get; }
}