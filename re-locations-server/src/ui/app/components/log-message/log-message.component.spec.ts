import { ComponentFixture, TestBed, waitForAsync } from '@angular/core/testing';

import { LogMessageComponent } from './log-message.component';

describe('LogMessageComponent', () => {
  let component: LogMessageComponent;
  let fixture: ComponentFixture<LogMessageComponent>;

  beforeEach(waitForAsync(() => {
    TestBed.configureTestingModule({
      declarations: [ LogMessageComponent ]
    })
    .compileComponents();
  }));

  beforeEach(() => {
    fixture = TestBed.createComponent(LogMessageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
