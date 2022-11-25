using System.Text.Json.Serialization;
namespace Discord
{
    public enum EmbedError
    {
        TitleTooLong,
        DescriptionTooLong,
        TooManyFields,
        FieldNameTooLong,
        FieldContentTooLong,
        FooterTextTooLong,
        AuthorNameToolong
    }
}

