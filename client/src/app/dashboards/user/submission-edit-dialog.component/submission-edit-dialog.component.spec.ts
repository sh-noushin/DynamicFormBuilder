import { ComponentFixture, TestBed } from '@angular/core/testing';

import { SubmissionEditDialogComponent } from './submission-edit-dialog.component';

describe('SubmissionEditDialogComponent', () => {
  let component: SubmissionEditDialogComponent;
  let fixture: ComponentFixture<SubmissionEditDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [SubmissionEditDialogComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(SubmissionEditDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
