import { ComponentFixture, TestBed } from '@angular/core/testing';

import { EditVersionDialogComponent } from './edit-version-dialog.component';

describe('EditVersionDialogComponent', () => {
  let component: EditVersionDialogComponent;
  let fixture: ComponentFixture<EditVersionDialogComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [EditVersionDialogComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(EditVersionDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
