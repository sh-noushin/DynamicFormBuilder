// Curated "field library" — one-click prebuilt field sets for common form
// patterns. Purely client-side; each template inflates into a sequence of
// CreateFormFieldDto POSTs against the existing field-creation endpoint,
// so no backend or wire-format change is needed.
//
// Keep template names / labels neutral and lower-case-friendly - they get
// suffix-deduped when they collide with an existing field name on the form.
export interface FieldTemplateField {
  name: string;
  label: string;
  type: string;
  isRequired?: boolean;
  placeholder?: string;
  helpText?: string;
  // JSON string matching the Validation blob understood by the backend rules.
  validation?: string;
  // JSON string for Select/Radio option lists (label/value pairs).
  options?: string;
}

export interface FieldTemplate {
  id: string;
  name: string;
  icon: string;
  description: string;
  fields: FieldTemplateField[];
}

export const FIELD_TEMPLATES: FieldTemplate[] = [
  {
    id: 'contact-info',
    name: 'Contact info',
    icon: 'contact_page',
    description: 'Full name, email, phone.',
    fields: [
      { name: 'full_name', label: 'Full name', type: 'Text', isRequired: true, placeholder: 'Jane Doe' },
      { name: 'email', label: 'Email address', type: 'Email', isRequired: true, placeholder: 'jane@example.com' },
      { name: 'phone', label: 'Phone', type: 'Phone', placeholder: '+1 555 0100' },
    ],
  },
  {
    id: 'address',
    name: 'Address',
    icon: 'home',
    description: 'Street, city, state, postal code, country.',
    fields: [
      { name: 'street', label: 'Street', type: 'Text', isRequired: true },
      { name: 'city', label: 'City', type: 'Text', isRequired: true },
      { name: 'state', label: 'State / Region', type: 'Text' },
      { name: 'postal_code', label: 'Postal code', type: 'Text', isRequired: true },
      { name: 'country', label: 'Country', type: 'Text', isRequired: true },
    ],
  },
  {
    id: 'nps',
    name: 'NPS score',
    icon: 'sentiment_satisfied',
    description: 'A rating question plus a follow-up "what could we do better".',
    fields: [
      { name: 'nps_score', label: 'How likely are you to recommend us?', type: 'Rating', isRequired: true },
      { name: 'nps_reason', label: 'What could we do better?', type: 'Textarea', helpText: 'Optional but appreciated.' },
    ],
  },
  {
    id: 'feedback',
    name: 'Feedback rating',
    icon: 'star_rate',
    description: 'Star rating plus optional comments.',
    fields: [
      { name: 'rating', label: 'How would you rate your experience?', type: 'Rating', isRequired: true },
      { name: 'comments', label: 'Any comments?', type: 'Textarea' },
    ],
  },
  {
    id: 'consent',
    name: 'Consent',
    icon: 'verified',
    description: 'Required opt-in checkbox for terms or marketing.',
    fields: [
      { name: 'accept_terms', label: 'I agree to the terms and conditions', type: 'Checkbox', isRequired: true },
    ],
  },
];

// Returns the input name if it is free on the form, otherwise appends _2, _3,
// ... until it finds a free slot. `existing` is expected to be lower-cased for
// case-insensitive comparison.
export function uniquifyName(base: string, existing: Set<string>): string {
  const lower = base.toLowerCase();
  if (!existing.has(lower)) {
    existing.add(lower);
    return base;
  }
  for (let i = 2; i < 1000; i++) {
    const candidate = `${base}_${i}`;
    if (!existing.has(candidate.toLowerCase())) {
      existing.add(candidate.toLowerCase());
      return candidate;
    }
  }
  // Extreme collision fallback - practically unreachable.
  const fallback = `${base}_${Date.now()}`;
  existing.add(fallback.toLowerCase());
  return fallback;
}
