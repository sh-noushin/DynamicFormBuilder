import { CommonModule } from '@angular/common';
import { ChangeDetectionStrategy, Component } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { RouterModule } from '@angular/router';

@Component({
  selector: 'app-landing',
  standalone: true,
  imports: [CommonModule, RouterModule, MatIconModule],
  templateUrl: './landing.component.html',
  styleUrl: './landing.component.scss',
  changeDetection: ChangeDetectionStrategy.Eager,
})
export class LandingComponent {
  // Kept in the component so a future edit doesn't need to touch the
  // template. The email lands in your inbox; wire it to a proper CRM
  // once you have real inbound.
  readonly contactEmail = 'hello@formbuilder.example';

  readonly features = [
    {
      icon: 'dashboard_customize',
      title: 'Drag-and-drop builder',
      body: 'Build any form in minutes with a WYSIWYG canvas. 16 field types, page breaks, conditional logic.',
    },
    {
      icon: 'insights',
      title: 'Real submissions dashboard',
      body: 'See every response as it lands, filter by field, tag entries, export to CSV.',
    },
    {
      icon: 'webhook',
      title: 'Webhooks + Slack',
      body: 'Pipe submissions into any URL with HMAC signing. One-click Slack format for team channels.',
    },
    {
      icon: 'workspaces',
      title: 'Multi-workspace',
      body: 'Each customer gets an isolated tenant with their own users, forms, and billing.',
    },
    {
      icon: 'lock',
      title: 'Password-protected forms',
      body: 'Gate any form behind a shared password so only invited respondents can submit.',
    },
    {
      icon: 'api',
      title: 'REST API + API keys',
      body: 'Automate everything. Zapier / n8n / your own scripts via a stable, documented API.',
    },
  ];

  readonly plans = [
    {
      name: 'Free',
      price: '$0',
      period: 'forever',
      cta: 'Sign in',
      ctaLink: '/login',
      ctaExternal: false,
      highlight: false,
      features: [
        '3 forms',
        '100 submissions / month',
        'Public form URL',
        'CSV export',
      ],
    },
    {
      name: 'Pro',
      price: '$19',
      period: 'per workspace / month',
      cta: 'Contact sales',
      ctaLink: '', // set at runtime via contactEmail
      ctaExternal: true,
      highlight: true,
      features: [
        '25 forms',
        '5,000 submissions / month',
        'Webhooks + Slack',
        'Priority email support',
      ],
    },
  ];

  get contactMailto(): string {
    return `mailto:${this.contactEmail}?subject=FormBuilder%20Pro%20-%20new%20workspace`;
  }
}
