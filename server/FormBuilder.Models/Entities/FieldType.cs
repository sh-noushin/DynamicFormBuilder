namespace FormBuilder.Models.Entities;

public enum FieldType
{
    Text,
    Email,
    Number,
    Date,
    DateTime,
    Checkbox,
    Radio,
    Select,
    Textarea,
    Phone,
    Password,
    File,
    Signature,
    Rating,
    PageBreak,
    // Never rendered on the public form. Its value comes from URL prefill
    // (?field_name=value) or the field's DefaultValue and is submitted
    // like any other field so admins can capture UTM / campaign / referrer
    // tags without the visitor seeing anything.
    HiddenField
}
