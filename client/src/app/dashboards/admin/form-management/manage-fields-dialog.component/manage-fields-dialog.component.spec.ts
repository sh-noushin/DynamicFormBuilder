import { ComponentFixture, TestBed } from '@angular/core/testing';

import { ManageFieldsDialogComponent } from './manage-fields-dialog.component';

describe('ManageFieldsDialogComponent', () => {
  let component: ManageFieldsDialogComponent;
  let fixture: ComponentFixture<ManageFieldsDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ManageFieldsDialogComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ManageFieldsDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
