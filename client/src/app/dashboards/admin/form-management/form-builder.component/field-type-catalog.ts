// The palette of field types the admin can drag onto the canvas. Order
// here dictates the order they appear in the toolbox. Every entry maps
// 1:1 to a FieldType enum value on the backend.
export interface FieldTypeEntry {
  type: string;
  label: string;
  icon: string;
  description: string;
  // A friendly default label the field gets when dropped onto the canvas.
  // The admin can rename it via the properties panel.
  defaultLabel: string;
}

export const FIELD_TYPE_CATALOG: FieldTypeEntry[] = [
  { type: 'Text',        label: 'Short text',   icon: 'short_text',        description: 'Single-line free text',        defaultLabel: 'Short answer' },
  { type: 'Textarea',    label: 'Long text',    icon: 'notes',             description: 'Multi-line text area',         defaultLabel: 'Long answer' },
  { type: 'Email',       label: 'Email',        icon: 'alternate_email',   description: 'Validated email address',      defaultLabel: 'Email address' },
  { type: 'Number',      label: 'Number',       icon: 'looks_one',         description: 'Numeric input with min / max', defaultLabel: 'Number' },
  { type: 'Phone',       label: 'Phone',        icon: 'phone',             description: 'Phone number',                 defaultLabel: 'Phone number' },
  { type: 'Date',        label: 'Date',         icon: 'event',             description: 'Calendar date',                defaultLabel: 'Date' },
  { type: 'DateTime',    label: 'Date & time',  icon: 'event_note',        description: 'Date + time picker',           defaultLabel: 'Date & time' },
  { type: 'Password',    label: 'Password',     icon: 'lock',              description: 'Masked text input',            defaultLabel: 'Password' },
  { type: 'Radio',       label: 'Multiple choice', icon: 'radio_button_checked', description: 'Pick one from options', defaultLabel: 'Choose one' },
  { type: 'Select',      label: 'Dropdown',     icon: 'arrow_drop_down_circle', description: 'Dropdown picker',        defaultLabel: 'Select an option' },
  { type: 'Checkbox',    label: 'Checkbox',     icon: 'check_box',         description: 'Yes / no toggle',              defaultLabel: 'I agree' },
  { type: 'Rating',      label: 'Rating',       icon: 'star',              description: '1 to 5 star rating',           defaultLabel: 'How would you rate this?' },
  { type: 'File',        label: 'File upload',  icon: 'attach_file',       description: 'Attach a file',                defaultLabel: 'Upload a file' },
  { type: 'Signature',   label: 'Signature',    icon: 'draw',              description: 'Drawn signature pad',          defaultLabel: 'Sign here' },
  { type: 'HiddenField', label: 'Hidden',       icon: 'visibility_off',    description: 'Hidden value (URL prefill)',   defaultLabel: 'hidden_value' },
  { type: 'PageBreak',   label: 'Page break',   icon: 'insert_page_break', description: 'Split into a new page',        defaultLabel: 'Next page' },
];
