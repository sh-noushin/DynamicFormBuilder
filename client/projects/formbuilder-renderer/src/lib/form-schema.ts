// Type definitions for the JSON produced by FormBuilder's "Export as JSON"
// button (GET /api/forms/{id}/export). These mirror FormExportDto on the
// server; keep them in sync when that DTO changes and bump
// FB_SUPPORTED_FORMAT_VERSION on any breaking change.

export type FbFieldType =
  | 'Text'
  | 'Email'
  | 'Number'
  | 'Date'
  | 'DateTime'
  | 'Checkbox'
  | 'Radio'
  | 'Select'
  | 'Textarea'
  | 'Phone'
  | 'Password'
  | 'File'
  | 'Signature'
  | 'Rating'
  | 'PageBreak'
  | 'HiddenField';

// Field types this renderer draws. File and Signature are recognised but not
// rendered — both need FormBuilder's upload endpoint and a canvas widget, so
// they arrive in a later version.
export const FB_RENDERABLE_TYPES: readonly FbFieldType[] = [
  'Text', 'Email', 'Number', 'Date', 'DateTime', 'Checkbox', 'Radio',
  'Select', 'Textarea', 'Phone', 'Password', 'Rating',
];

export interface FbFieldOption {
  value: string;
  label: string;
}

export interface FbField {
  name: string;
  label: string;
  // Kept loose because the export writes the enum as a plain string; unknown
  // values are skipped rather than crashing the render.
  type: FbFieldType | string;
  order?: number;
  isRequired?: boolean;
  isVisible?: boolean;
  isReadOnly?: boolean;
  placeholder?: string | null;
  helpText?: string | null;
  defaultValue?: string | null;
  // JSON string. Either a bare regex (legacy) or an object of rules —
  // see parseValidation().
  validation?: string | null;
  // JSON string: array of strings, or of { value, label } pairs.
  options?: string | null;
  // JSON string: {"field":"otherFieldName","equals":"value"}.
  showIfCondition?: string | null;
}

export interface FbFormBody {
  name?: string;
  description?: string | null;
  brandColor?: string | null;
  thankYouMessage?: string | null;
  fields: FbField[];
}

export interface FbFormSchema {
  formatVersion?: number;
  exportedAt?: string;
  form: FbFormBody;
}

// Highest export format this package understands. A schema declaring a
// higher number is rejected rather than half-rendered, because unknown
// fields would silently disappear from the form.
export const FB_SUPPORTED_FORMAT_VERSION = 1;

export class FbSchemaError extends Error {
  constructor(message: string) {
    super(message);
    this.name = 'FbSchemaError';
  }
}

// Accepts any of the three shapes people realistically paste in:
//   1. a full export      { formatVersion, exportedAt, form: { fields: [] } }
//   2. just the form body { name, fields: [] }
//   3. a bare field array [ { name, label, type } ]
// and returns a normalized body with fields sorted by `order`.
export function normalizeSchema(input: unknown): FbFormBody {
  if (input == null) {
    throw new FbSchemaError('No schema provided.');
  }

  let body: unknown = input;

  if (Array.isArray(input)) {
    body = { fields: input };
  } else if (typeof input === 'object') {
    const candidate = input as Partial<FbFormSchema> & Partial<FbFormBody>;
    if (candidate.form && typeof candidate.form === 'object') {
      const version = candidate.formatVersion ?? FB_SUPPORTED_FORMAT_VERSION;
      if (version > FB_SUPPORTED_FORMAT_VERSION) {
        throw new FbSchemaError(
          `Schema formatVersion ${version} is newer than this renderer supports ` +
          `(${FB_SUPPORTED_FORMAT_VERSION}). Update @formbuilder/renderer.`
        );
      }
      body = candidate.form;
    }
  } else {
    throw new FbSchemaError('Schema must be an object or an array of fields.');
  }

  const result = body as FbFormBody;
  if (!Array.isArray(result?.fields)) {
    throw new FbSchemaError('Schema has no "fields" array.');
  }

  return {
    ...result,
    fields: [...result.fields].sort((a, b) => (a.order ?? 0) - (b.order ?? 0)),
  };
}
