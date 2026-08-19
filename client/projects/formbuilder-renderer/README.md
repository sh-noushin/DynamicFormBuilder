# formbuilder-renderer

Render a FormBuilder form from its exported JSON, inside any Angular app.

Export a form from the FormBuilder dashboard (**Forms → ⬇ Export as JSON**), hand the file to
`<fb-form>`, and you get a working, validated form. No iframe, no FormBuilder server required.

## Install

```bash
npm install formbuilder-renderer
```

Peer dependencies: `@angular/core`, `@angular/common`, `@angular/forms` (v22+).

## Usage

### Standalone — you keep the data

```ts
import { FbFormComponent, FbSubmitEvent } from 'formbuilder-renderer';
import contactForm from './contact-form.json';

@Component({
  standalone: true,
  imports: [FbFormComponent],
  template: `<fb-form [schema]="schema" (submitted)="save($event)"></fb-form>`,
})
export class MyPage {
  schema = contactForm;

  save(event: FbSubmitEvent) {
    // event.data === { firstName: 'Ada', email: 'ada@example.com', ... }
    this.http.post('/my/own/api', event.data).subscribe();
  }
}
```

Nothing is sent anywhere. The JSON file is the only input.

### Post-back — answers land in FormBuilder

```html
<fb-form
  [schema]="schema"
  apiBaseUrl="https://forms.example.com"
  submitToSlug="a1b2c3"
  (submitted)="onDone($event)"
  (submitError)="onError($event)">
</fb-form>
```

The component POSTs to `/api/public/forms/{slug}/submissions`, so the response appears in the
workspace dashboard with analytics, CSV export, webhooks, and confirmation emails — exactly as if
the visitor had used the hosted public link.

## API

| Input | Type | Default | Notes |
|---|---|---|---|
| `schema` | `unknown` | — | Required. Full export, form body, or bare field array. |
| `apiBaseUrl` | `string \| null` | `null` | FormBuilder origin. Required only for post-back. |
| `submitToSlug` | `string \| null` | `null` | Form slug. When set, answers are POSTed. |
| `submitLabel` | `string` | `'Submit'` | Text on the submit button. |
| `collectSubmitter` | `boolean` | `false` | Adds name/email fields, as the hosted form has. |

| Output | Payload | Fires when |
|---|---|---|
| `submitted` | `{ data, posted }` | The form validates (and posts, if configured). |
| `validationFailed` | — | Submit or Next was blocked by an invalid field. |
| `submitError` | `{ status, message }` | The POST failed. |

`data` is keyed by **field name** — the same keys FormBuilder's API and CSV export use. Fields
hidden by a `showIfCondition` are omitted, matching server behaviour.

## Supported field types

Rendered: `Text`, `Email`, `Number`, `Date`, `DateTime`, `Checkbox`, `Radio`, `Select`,
`Textarea`, `Phone`, `Password`, `Rating`.

Structural: `PageBreak` splits the form into pages with Back/Next; `HiddenField` is never drawn
but its default value is submitted.

Not yet supported: `File` and `Signature`. Both are recognised and shown as a placeholder note
rather than silently dropped — file upload needs FormBuilder's upload endpoint, so it cannot work
from a bare JSON file.

Also honoured: `isRequired`, `isReadOnly`, `isVisible`, `placeholder`, `helpText`, `defaultValue`,
`options`, `showIfCondition`, and `validation` (min/max ranges with `rangeMessage`, or a bare
regex pattern).

## Styling

No Material, no theme import. Override CSS custom properties:

```css
fb-form {
  --fb-brand: #0ea5e9;
  --fb-text: #0f172a;
  --fb-border: #cbd5e1;
  --fb-danger: #dc2626;
  --fb-radius: 12px;
}
```

Every element also carries a plain `fb-*` class (`.fb-field`, `.fb-input`, `.fb-button`, …) if you
want to restyle it completely.

## Schema compatibility

The package understands export `formatVersion` 1. A schema declaring a newer version is rejected
with a visible message rather than partially rendered, so no field ever disappears silently.

## Local development

This library lives in the FormBuilder workspace. Build it before building the host app:

```bash
ng build formbuilder-renderer
ng build            # the app resolves 'formbuilder-renderer' from dist/
```

A live demo is at `/playground` in the FormBuilder app: paste a schema, click **Render form**.
